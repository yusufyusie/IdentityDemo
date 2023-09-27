﻿using AutoMapper;
using IdentityDemo.DTOs;
using IdentityDemo.model;

namespace IdentityDemo.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // CreateMap<Source, Destination>();
            // user
            CreateMap<ApplicationUser, UserDTO>(); ;
            CreateMap<UserForRegistrationDto, ApplicationUser>();
            CreateMap<UserCredentials, ApplicationUser>();
            // role
            CreateMap<ApplicationRole, RoleDto>();
            CreateMap<CreateOrUpdateRoleDto, ApplicationRole>();
            // user role
            CreateMap<ApplicationUserRole, UserRoleDto>();
        }
    }
}
