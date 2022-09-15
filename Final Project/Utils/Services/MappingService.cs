using AutoMapper;
using Final_Project.Models;
using Final_Project.Requests.UserRequests;
using Final_Project.Requests.RoleRequests;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.ItemTypeRequest;
using Final_Project.Requests.OderRequests;

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
            CreateMap<UpdateUserRequest, UserModel>().ReverseMap();

            //Roles
            CreateMap<AddRoleRequest, RoleModel>().ReverseMap();

            //Item
            CreateMap<AddItemRequest, ItemModel>().ReverseMap();
            CreateMap<UpdateItemRequests, ItemModel>().ReverseMap();

            //Itemtype
            CreateMap<AddItemTypeRequest, ItemTypeModel>().ReverseMap();

            //Oder
            CreateMap<CreateOderRequest, OderModel>().ReverseMap();
            CreateMap<UpdateOderRequest, OderModel>().ReverseMap(); 
        }
    }
}
