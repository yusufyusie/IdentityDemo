using AutoMapper;
using IdentityDemo.DTOs;
using IdentityDemo.model;

namespace IdentityDemo.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<ApplicationUser, UserDTO>(); ;
            CreateMap<UserForRegistrationDto, ApplicationUser>();
            CreateMap<UserCredentials, ApplicationUser>();
        }
    }
}
