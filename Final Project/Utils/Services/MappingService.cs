using AutoMapper;
using Final_Project.Models;
using Final_Project.Requests.UserRequests;
using Final_Project.Requests.RoleRequests;
using Final_Project.Requests.Itemrequests;
using Final_Project.Requests.UpdateItemRequests;
using Final_Project.Requests.ItemTypeRequest;
using Final_Project.Requests.ToppingRequests;
using Final_Project.Requests.OrderRequests;

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
            //CreateMap<AddItemImageRequest, ItemModel>().ReverseMap();

            //Itemtype
            CreateMap<AddItemTypeRequest, ItemTypeModel>().ReverseMap();
            CreateMap<UpdateTypeRequest, ItemTypeModel>().ReverseMap();

            //Order
            CreateMap<CreateOrderRequest, OrderModel>().ReverseMap();
            CreateMap<UpdateOrderRequest, OrderModel>().ReverseMap();

            //Topping
            CreateMap<AddToppingRequest, ToppingModel>().ReverseMap();
            CreateMap<UpdateToppingRequest, ToppingModel>().ReverseMap();
        }
    }
}
