using AutoMapper;
using AutoMapper.Execution;
using Week4.Dtos;
using Week4.Models;

namespace Week4.Resolvers
{
    public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            var daysOld = (DateTime.UtcNow - source.PublishedDate).TotalDays;

            if (daysOld < 30)
                return "New Release";
            else if (daysOld < 365)
                return $"{Math.Floor(daysOld / 30)} months old";
            else if (daysOld < 1825)
                return $"{Math.Floor(daysOld / 365)} years old";
            else if (Math.Abs(daysOld - 1825) < 1)
                return "Classic";
            else
                return $"{Math.Floor(daysOld / 365)} years old";
        }
    }
}
