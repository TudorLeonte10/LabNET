using AutoMapper;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Week4.Common.Mapping;
using Week4.Data;
using Week4.Features.Orders;
using Week4.Features.Orders.Commands;
using Week4.Features.Orders.DTOs;
using Xunit;

namespace Week4.Tests
{
    public class CreateOrderHandlerIntegrationTests : IDisposable
    {
        private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly ApplicationContext _context;
        private readonly IMemoryCache _cache;
        private readonly CreateOrderHandler _handler;
        private readonly IMapper _mapper;

        public CreateOrderHandlerIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationContext(options);

            _cache = new MemoryCache(new MemoryCacheOptions());

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AdvancedOrderMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            var loggerMock = new Mock<ILogger<CreateOrderHandler>>();
            _logger = loggerMock.Object;
            _loggerMock = loggerMock;

            _handler = new CreateOrderHandler(_context, _mapper, _cache, _logger);
        }

        [Fact]
        public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
        {
            var request = new CreateOrderProfileRequest
            {
                Title = "Learning C# Programming",
                Author = "Jane Doe",
                ISBN = "978-3-16-148410-0",
                Category = OrderCategory.Technical,
                Price = 59.99m,
                PublishedDate = DateTime.UtcNow.AddDays(-15),
                CoverImageUrl = "https://example.com/cover.jpg",
                StockQuantity = 10
            };

            var command = new CreateOrderCommand(request);
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result); 
            Assert.Equal("Learning C# Programming", result.Title);
            Assert.Equal("JD", result.AuthorInitials);
            Assert.Equal("New Release", result.PublishedAge);
            Assert.StartsWith("$", result.FormattedPrice);
            Assert.Equal("In Stock", result.AvailabilityStatus);

            _loggerMock.Verify(
                l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 2001), 
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Starting order creation")), 
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
                Times.Once 
                );

        }

        [Fact]
        public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
        {
            var existingOrder = new Order
            {
                Title = "Existing Order",
                Author = "John Smith",
                ISBN = "1234567891",
                Category = OrderCategory.Fiction,
                Price = 29.99m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 5,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(existingOrder);
            await _context.SaveChangesAsync();

            var duplicateRequest = new CreateOrderProfileRequest
            {
                Title = "Duplicate Book",
                Author = "Jane Doe",
                ISBN = "1234567891",
                Category = OrderCategory.NonFiction,
                Price = 25.99m,
                PublishedDate = DateTime.UtcNow.AddMonths(-6),
                StockQuantity = 3
            };

            var command = new CreateOrderCommand(duplicateRequest);
            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.Null(result);

            _loggerMock.Verify(
                l => l.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 2002),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Validation failed")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
        {
            var request = new CreateOrderProfileRequest
            {
                Title = "Adventures in Wonderland",
                Author = "Lewis Carroll",
                ISBN = "978-1-23456-789-0",
                Category = OrderCategory.Children,
                Price = 40.00m,
                PublishedDate = DateTime.UtcNow.AddYears(-3),
                StockQuantity = 15
            };

            var command = new CreateOrderCommand(request);
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Children's Orders", result!.CategoryDisplayName);
            Assert.Equal(36.00m, result.Price);
            Assert.Null(result.CoverImageUrl);

        }

        public void Dispose()
        {
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
