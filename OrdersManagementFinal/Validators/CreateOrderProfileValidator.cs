using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using Week4.Data;
using Week4.Features.Orders;
using Week4.Features.Orders.DTOs;

namespace Week4.Validators
{
    public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<CreateOrderProfileValidator> _logger;

        public CreateOrderProfileValidator(ApplicationContext context, ILogger<CreateOrderProfileValidator> logger)
        {
            _context = context;
            _logger = logger;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .MinimumLength(1).WithMessage("Title must be at least 1 character long.")
                .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
                .MustAsync(BeUniqueTitle).WithMessage("An order with the same title already exists.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MinimumLength(2)
                .MaximumLength(100)
                .Matches(@"^[a-zA-Z\s\-\.'’]+$").WithMessage("Author name contains invalid characters.");

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN is required.")
                .Matches(@"^(?:\d{9}[\dXx]|\d{13})$").WithMessage("Invalid ISBN format (must be 10 or 13 digits).")
                .MustAsync(BeUniqueIsbn).WithMessage("The Isbn is not unique");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid order category.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThan(10000).WithMessage("Price must be less than $10,000.");

            RuleFor(x => x.PublishedDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
                .GreaterThan(new DateTime(1400, 1, 1)).WithMessage("Published date cannot be before year 1400.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
                .LessThanOrEqualTo(100000).WithMessage("Stock quantity exceeds reasonable limits.");

            When(x => !string.IsNullOrEmpty(x.CoverImageUrl), () =>
            {
                RuleFor(x => x.CoverImageUrl)
                    .Must(BeValidImageUrl).WithMessage("Invalid image URL format.");
            });

            RuleFor(x => x).MustAsync(PassBusinessRules)
                .WithMessage("Business rule validation failed.");

            When(x => x.Category == OrderCategory.Technical, () =>
            {
                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(20).WithMessage("Technical orders must have a minimum price of $20.");
                RuleFor(t => t.Title)
                    .Must(ContainTechnicalWords).WithMessage("Technical order titles must contain technical terms.");
                RuleFor(p => p.PublishedDate)
                    .Must(BePublishedWithinLastFiveYears).WithMessage("Technical orders must be published within the last 5 years.");
            });

            When(x => x.Category == OrderCategory.Children, () =>
            {
                RuleFor(t => t.Title)
                    .Must(BeValidTitle).WithMessage("Children's order titles cannot contain inappropriate content.");
                RuleFor(p => p.Price)
                    .LessThanOrEqualTo(50).WithMessage("Children's orders cannot exceed $50 in price.");
            });

            When(x => x.Category == OrderCategory.Fiction, () =>
            {
                RuleFor(a => a.Author)
                    .MinimumLength(5).WithMessage("Fiction order authors must have names longer than 4 characters.");
            });

            RuleFor(x => x)
                .Must(x => x.Price <= 100 || x.StockQuantity <= 20)
                .WithMessage("Expensive orders (> $100) must have limited stock (≤ 20 units).");

            RuleFor(x => x)
                .Must(x => x.Category != OrderCategory.Technical || BePublishedWithinLastFiveYears(x.PublishedDate))
                .WithMessage("Technical orders must be recent (published within last 5 years).");
        }

        private bool BeValidTitle(string title)
        {
            string[] badWords = {"violence", "drugs", "gambling", "explicit"};

            return !badWords.Any(b => title.Contains(b));
        }

        private bool BePublishedWithinLastFiveYears(DateTime publishedDate)
        {
            return publishedDate >= DateTime.UtcNow.AddYears(-5);
        }

        private bool ContainTechnicalWords(string title)
        {
            string[] technicalWords = { "programming", "software", "hardware", "network", "database", "algorithm", "development", "engineering" };
            return technicalWords.Any(t => title.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)
        {
            bool exists = await _context.Orders.AnyAsync(o => o.Title == title, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Validation failed: Title {Title} already exists.", title);
            }
            return !exists;
        }

        private async Task<bool> BeUniqueIsbn(string isbn, CancellationToken token)
        {
            bool exists = await _context.Orders.AnyAsync(i => i.ISBN == isbn, token);
            if (exists)
            {
                _logger.LogWarning("Validation failed: Isbn {ISBN} already exists", isbn);
            }
            return !exists;
        }

        private bool BeValidImageUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                   && (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png")
                       || url.EndsWith(".gif") || url.EndsWith(".webp"));
        }

        private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken ct)
        {
            var todayCount = await _context.Orders
                .CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date, ct);
            if (todayCount > 500)
            {
                _logger.LogWarning("Daily order limit exceeded (500).");
                return false;
            }

            if (request.Category == OrderCategory.Technical && request.Price < 20)
            {
                _logger.LogWarning("Technical order price too low: ${Price}", request.Price);
                return false;
            }

            if (request.Category == OrderCategory.Children && request.Title.Contains("violence", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Children's order contains restricted words: {Title}", request.Title);
                return false;
            }

            if (request.Price > 500 && request.StockQuantity > 10)
            {
                _logger.LogWarning("High-value order exceeds stock limit: Price={Price}, Stock={Stock}", request.Price, request.StockQuantity);
                return false;
            }

            return true;
        }
    }
}
