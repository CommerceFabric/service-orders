using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Mappers
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<OrderAddRequest, Order>();
            CreateMap<OrderUpdateRequest, Order>();
            CreateMap<Order, OrderResponse>();
        }
    }
}
