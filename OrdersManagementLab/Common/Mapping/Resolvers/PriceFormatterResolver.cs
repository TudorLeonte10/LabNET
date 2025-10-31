using AutoMapper.Execution;

using AutoMapper;
using Week4.Features.Orders.DTOs;
using Week4.Features.Orders;

namespace Week4.Common.Mapping.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto orderProfileDto, string destMember, ResolutionContext context)
        {
            return source.Price.ToString("C2");
        }
    }
}
