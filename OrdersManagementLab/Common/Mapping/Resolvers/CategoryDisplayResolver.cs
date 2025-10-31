﻿using AutoMapper;
using Week4.Features.Orders;
using Week4.Features.Orders.DTOs;


namespace Week4.Common.Mapping.Resolvers
{
    public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            return source.Category switch
            {
                OrderCategory.Fiction => "Fiction & Literature",
                OrderCategory.NonFiction => "Non-Fiction",
                OrderCategory.Technical => "Technical & Professional",
                OrderCategory.Children => "Children's Orders",
                _ => "Uncategorized"
            };
        }
    }
}
