using DatingApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    /// <summary>
    /// This repository is responsible to query from database via entityFramework
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _Context;

        public AuthRepository(DataContext dataContext)
        {
            this._Context = dataContext;
        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }


        /// <summary>
        /// used to verify pass hash 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        /// <returns></returns>
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i=0; i<computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            };
            return true;
        }

        public async Task<User> Register(User user, string Password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _Context.Users.AddAsync(user);
            await _Context.SaveChangesAsync();
            return user;
        }


        /// <summary>
        /// generate salt and pass hash for a pass
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Since parent abstract class HashAlgorithm implement IDisposible interface so we can use using statement to call dispose automatically
            // after using block
            using(var hmac = new System.Security.Cryptography.HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            };
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _Context.Users.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }
    }
}
