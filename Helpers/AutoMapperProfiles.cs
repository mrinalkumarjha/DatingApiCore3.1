﻿using AutoMapper;
using DatingApp.Dtos;
using DatingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            // ForMember is used here to fill age and photourl
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt =>
                {
                    opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<User, UserForDetailDto>()
                 .ForMember(dest => dest.PhotoUrl, opt =>
                 {
                     opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                 })
                .ForMember(dest => dest.Age, opt =>
                {
                    opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
        }
    }
}
