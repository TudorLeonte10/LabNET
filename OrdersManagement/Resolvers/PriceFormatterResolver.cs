using AutoMapper.Execution;
using Week4.Dtos;
using Week4.Models;
using AutoMapper;

namespace Week4.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto orderProfileDto, string destMember, ResolutionContext context)
        {
            return source.Price.ToString("C2");
        }
    }
}
