using System;
using System.ComponentModel.DataAnnotations;
using Week4.Validators.Attributes;

namespace Week4.Features.Orders.DTOs
{
    public class CreateOrderProfileRequest
    {
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;

        [ValidISBN]
        public string ISBN { get; set; } = string.Empty;

        [OrderCategory(OrderCategory.Fiction, OrderCategory.NonFiction, OrderCategory.Children, OrderCategory.Technical)]
        public OrderCategory Category { get; set; }

        [PriceRange(0.01, 10000)]
        public decimal Price { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? CoverImageUrl { get; set; }
        public int StockQuantity { get; set; } = 1;
    }

    public class OrderProfileDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }     
        public DateTime CreatedAt { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }
        public string PublishedAge { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = string.Empty;
        public string AvailabilityStatus { get; set; } = string.Empty;
    }
}
