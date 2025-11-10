using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Week4.Features.Orders.Commands;
using Week4.Features.Orders.DTOs;
using Week4.Features.Orders.Queries;

namespace Week4.Features.Orders
{
    public static class OrdersEndpoints
    {
        public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/orders").WithTags("Orders");

            group.MapPost("/", async (CreateOrderProfileRequest request, IMediator mediator, IValidator<CreateOrderProfileRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    return Results.BadRequest(errors);
                }

                var result = await mediator.Send(new CreateOrderCommand(request));
                return result == null
                    ? Results.BadRequest(new { message = $"Order with ISBN {request.ISBN} already exists." })
                    : Results.Created($"/api/orders/{result.ISBN}", result);
            });

            group.MapGet("/", async (IMediator mediator) =>
            {
                var orders = await mediator.Send(new GetAllOrdersQuery());
                return Results.Ok(orders);
            });

   
            group.MapGet("/{isbn}", async (string isbn, IMediator mediator) =>
            {
                var order = await mediator.Send(new GetOrderByIsbnQuery(isbn));
                return order is null
                    ? Results.NotFound(new { message = $"Order with ISBN {isbn} not found." })
                    : Results.Ok(order);
            });
        }
    }
}
