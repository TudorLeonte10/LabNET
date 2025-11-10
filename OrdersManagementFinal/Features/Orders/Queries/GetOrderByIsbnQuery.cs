using MediatR;
using Microsoft.EntityFrameworkCore;
using Week4.Data;

namespace Week4.Features.Orders.Queries
{
    public record GetOrderByIsbnQuery(string ISBN) : IRequest<Order?>;

    public class GetOrderByIsbnQueryHandler : IRequestHandler<GetOrderByIsbnQuery, Order?>
    {
        private readonly ApplicationContext _context;

        public GetOrderByIsbnQueryHandler(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<Order?> Handle(GetOrderByIsbnQuery request, CancellationToken cancellationToken)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.ISBN == request.ISBN, cancellationToken);
        }
    }
}
