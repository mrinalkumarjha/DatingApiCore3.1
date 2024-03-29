﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Data;
using DatingApp.Dtos;
using DatingApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Controllers
{
    /// <summary>
    /// ControllerBase is used as base class as it is api project and we dot need view for this.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController] // after using this we dont need to check modelstate explicitly , also no need to define [FromBody] explicitly as it maps automatically
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _confiuration;
        private readonly IMapper _mapper;

        /// <summary>
        /// injecting dependency in constructor
        /// </summary>
        public AuthController(IAuthRepository authRepository, IConfiguration confiuration, IMapper mapper)
        {
            _authRepository = authRepository;
            _confiuration = confiuration;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username= userForRegisterDto.Username.ToLower();
            if (await _authRepository.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            //var userToCcreate = new User
            //{
            //    Username = userForRegisterDto.Username
            //};
            // using automapper for above code

            var userToCcreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _authRepository.Register(userToCcreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailDto>(createdUser);

            return CreatedAtRoute("GetUser", new { Controller = "Users", id = createdUser.Id }, userToReturn);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // build token
            // claims hold user info
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_confiuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor); // this is our jwt token

            var user = _mapper.Map<UserForListDto>(userFromRepo); // for displaying user image in navbar

            // returning user in anonmus method
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            }); 
        }

    }
}