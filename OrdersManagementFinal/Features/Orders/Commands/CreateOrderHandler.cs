using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Week4.Common.Logging;
using Week4.Data;
using Week4.Features.Orders.DTOs;

namespace Week4.Features.Orders.Commands
{
    public record CreateOrderCommand(CreateOrderProfileRequest Request) : IRequest<OrderProfileDto?>;

    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderProfileDto?>
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

        public async Task<OrderProfileDto?> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var totalWatch = System.Diagnostics.Stopwatch.StartNew();

            using var scope = _logger.BeginScope("OrderOperation: {OperationId}", operationId);

            try
            {
                _logger.LogInformation(new EventId(LogEvents.OrderCreationStarted, nameof(LogEvents.OrderCreationStarted)),
                    "Starting order creation for Title={Title}, Author={Author}, Category={Category}, ISBN={ISBN}",
                    request.Title, request.Author, request.Category, request.ISBN);

                var validationWatch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation(new EventId(LogEvents.ISBNValidationPerformed, nameof(LogEvents.ISBNValidationPerformed)),
                    "Performing ISBN uniqueness validation for ISBN={ISBN}", request.ISBN);

                bool exists = await _context.Orders.AnyAsync(o => o.ISBN == request.ISBN, cancellationToken);
                if (exists)
                {
                    validationWatch.Stop();
                    var failMetrics = new OrderCreationMetrics(operationId, request.Title, request.ISBN, request.Category,
                        validationWatch.Elapsed, TimeSpan.Zero, validationWatch.Elapsed, false, "Duplicate ISBN");
                    _logger.LogOrderCreationMetrics(failMetrics);
                    _logger.LogWarning(new EventId(LogEvents.OrderValidationFailed, nameof(LogEvents.OrderValidationFailed)),
                        "Validation failed: ISBN {ISBN} already exists.", request.ISBN);
                    return null;
                }

                _logger.LogInformation(new EventId(LogEvents.StockValidationPerformed, nameof(LogEvents.StockValidationPerformed)),
                    "Performing stock validation for StockQuantity={StockQuantity}", request.StockQuantity);
                validationWatch.Stop();

                var dbWatch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationStarted, nameof(LogEvents.DatabaseOperationStarted)),
                    "Database operation started for order ISBN={ISBN}", request.ISBN);

                var order = _mapper.Map<Order>(request);
                await _context.Orders.AddAsync(order, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                dbWatch.Stop();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationCompleted, nameof(LogEvents.DatabaseOperationCompleted)),
                    "Database operation completed successfully for OrderId={OrderId}", order.Id);

                _cache.Remove("all_orders");
                _logger.LogInformation(new EventId(LogEvents.CacheOperationPerformed, nameof(LogEvents.CacheOperationPerformed)),
                    "Cache invalidated successfully (key='all_orders')");

                totalWatch.Stop();
                var successMetrics = new OrderCreationMetrics(
                    operationId,
                    order.Title,
                    order.ISBN,
                    order.Category,
                    validationWatch.Elapsed,
                    dbWatch.Elapsed,
                    totalWatch.Elapsed,
                    true
                );

                _logger.LogOrderCreationMetrics(successMetrics);
                return _mapper.Map<OrderProfileDto>(order);
            }
            catch (Exception ex)
            {
                totalWatch.Stop();
                _logger.LogError(ex, "Unhandled exception during order creation (OperationId={OperationId})", operationId);

                var errorMetrics = new OrderCreationMetrics(
                    operationId,
                    request.Title,
                    request.ISBN,
                    request.Category,
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    totalWatch.Elapsed,
                    false,
                    ex.Message
                );

                _logger.LogOrderCreationMetrics(errorMetrics);
                throw; 
            }
        }
    }
}
