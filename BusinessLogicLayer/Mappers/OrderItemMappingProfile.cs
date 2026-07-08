using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Mappers
{
    public class OrderItemMappingProfile : Profile
    {
        public OrderItemMappingProfile()
        {
            CreateMap<OrderItemAddRequest, OrderItem>();
            CreateMap<OrderItemUpdateRequest, OrderItem>();
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
            CreateMap<ProductDTO, OrderItemResponse>();
        }
    }
}
