using AutoMapper;
using KiwiToys.Data.Entities;
using KiwiToys.Models;

namespace KiwiToys.Services {
    public class AutoMapperProfile : Profile {
        public AutoMapperProfile() {
            CreateMap<User, EditUserViewModel>().ReverseMap();
            CreateMap<Product, AddProductToCartViewModel>().ReverseMap();
        }
    }
}