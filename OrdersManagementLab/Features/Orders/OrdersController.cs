using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Week4.Data;
using Week4.Features.Orders.DTOs;

namespace Week4.Features.Orders
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OrdersController> _logger;

        private const string CacheKey = "all_orders";

        public OrdersController(ApplicationContext context, CreateOrderHandler createOrderHandler, IMemoryCache cache, ILogger<OrdersController> logger)
        {
            _context = context;
            _createOrderHandler = createOrderHandler;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderProfileRequest request)
        {
            var result = await _createOrderHandler.HandleAsync(request);

            if (result == null)
            {
                return BadRequest(new { message = $"Order with ISBN {request.ISBN} already exists." });
            }

            return CreatedAtAction(nameof(GetByIsbn), new { isbn = result.ISBN }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<Order> orders))
            {
                orders = await _context.Orders.ToListAsync();
                _cache.Set(CacheKey, orders, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Loaded {Count} orders from database and cached them.", orders.Count());
            }
            else
            {
                _logger.LogInformation("Loaded {Count} orders from cache.", orders.Count());
            }

            return Ok(orders);
        }

        [HttpGet("{isbn}")]
        public async Task<IActionResult> GetByIsbn(string isbn)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.ISBN == isbn);

            if (order == null)
            {
                return NotFound(new { message = $"Order with ISBN {isbn} not found." });
            }

            return Ok(order);
        }
    }
}
