using AutoMapper;
using Final_Project.Models;
using Final_Project.Requests.UserRequests;
using Final_Project.Requests.RoleRequests;

namespace Final_Project.Utils.Services
{
    public class MappingService : Profile
    {
        public MappingService()
        {
            //Users
            CreateMap<LoginRequest, UserModel>().ReverseMap();
            CreateMap<RegisterRequest, UserModel>().ReverseMap();
            CreateMap<CreateRequest, UserModel>().ReverseMap();
            CreateMap<UpdateProfileRequest, UserModel>().ReverseMap();

            //Roles
            CreateMap<AddRoleRequest, RoleModel>().ReverseMap();
        }
    }
}
