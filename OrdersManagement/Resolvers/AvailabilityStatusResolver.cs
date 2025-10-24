using AutoMapper;
using System.Diagnostics.Eventing.Reader;
using Week4.Dtos;
using Week4.Models;

namespace Week4.Resolvers
{
    public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (!source.IsAvailable)
            {
                return "Unavailable";
            }
            else
            {
                if (source.StockQuantity == 0)
                {
                    return "Out of Stock";
                }
                else if (source.StockQuantity <= 5)
                {
                    return "Limited Stock";
                }
                else if (source.StockQuantity == 1)
                {
                    return "LastCopy";
                }
                else
                {
                    return "In Stock";
                }
            }
        }
    }
}
