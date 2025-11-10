using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Week4.Data;

namespace Week4.Features.Orders.Queries
{
    public record GetAllOrdersQuery : IRequest<IEnumerable<Order>>;

    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<Order>>
    {
        private readonly ApplicationContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GetAllOrdersQueryHandler> _logger;
        private const string CacheKey = "all_orders";

        public GetAllOrdersQueryHandler(ApplicationContext context, IMemoryCache cache, ILogger<GetAllOrdersQueryHandler> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<Order> ?orders))
            {
                orders = await _context.Orders.ToListAsync(cancellationToken);
                _cache.Set(CacheKey, orders, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Orders loaded from database.");
            }
            else
            {
                _logger.LogInformation("Orders loaded from cache.");
            }

            return orders!;
        }
    }
}
