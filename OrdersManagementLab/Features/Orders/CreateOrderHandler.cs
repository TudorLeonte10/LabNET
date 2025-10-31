
    using AutoMapper;
    using Week4.Common.Logging;
    using Week4.Data;
    using Week4.Features.Orders.DTOs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    namespace Week4.Features.Orders
    {
        public class CreateOrderHandler
        {
            private readonly ApplicationContext _context;
            private readonly IMapper _mapper;
            private readonly IMemoryCache _cache;
            private readonly ILogger<CreateOrderHandler> _logger;

            public CreateOrderHandler(ApplicationContext context, IMapper mapper, IMemoryCache cache, ILogger<CreateOrderHandler> logger)
            {
                _context = context;
                _mapper = mapper;
                _cache = cache;
                _logger = logger;
            }

            public async Task<OrderProfileDto?> HandleAsync(CreateOrderProfileRequest request)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                var totalWatch = System.Diagnostics.Stopwatch.StartNew();
                var validationWatch = System.Diagnostics.Stopwatch.StartNew();

                _logger.LogInformation(new EventId(LogEvents.OrderCreationStarted, nameof(LogEvents.OrderCreationStarted)),
                    "Starting order creation (OperationId={OperationId}) for Title={Title}, Author={Author}, Category={Category}, ISBN={ISBN}",
                    operationId, request.Title, request.Author, request.Category, request.ISBN);

                if (await _context.Orders.AnyAsync(o => o.ISBN == request.ISBN))
                {
                    validationWatch.Stop();
                    var failedMetrics = new OrderCreationMetrics(operationId, request.Title, request.ISBN, request.Category,
                        validationWatch.Elapsed, TimeSpan.Zero, validationWatch.Elapsed, false, "Duplicate ISBN");
                    _logger.LogOrderCreationMetrics(failedMetrics);
                    _logger.LogWarning(new EventId(LogEvents.OrderValidationFailed, nameof(LogEvents.OrderValidationFailed)),
                        "Validation failed: ISBN {ISBN} already exists.", request.ISBN);
                    return null;
                }

                validationWatch.Stop();

                var dbWatch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationStarted, nameof(LogEvents.DatabaseOperationStarted)),
                    "Saving order to database (ISBN={ISBN})", request.ISBN);

                var order = _mapper.Map<Order>(request);
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                dbWatch.Stop();
                _cache.Remove("all_orders");
                _logger.LogInformation(new EventId(LogEvents.CacheOperationPerformed, nameof(LogEvents.CacheOperationPerformed)),
                    "Cache invalidated (key='all_orders')");

                totalWatch.Stop();
                var metrics = new OrderCreationMetrics(operationId, order.Title, order.ISBN, order.Category,
                    validationWatch.Elapsed, dbWatch.Elapsed, totalWatch.Elapsed, true);

                _logger.LogOrderCreationMetrics(metrics);
                return _mapper.Map<OrderProfileDto>(order);
            }
        }
    }

