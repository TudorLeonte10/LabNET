using Microsoft.EntityFrameworkCore;
using Week4.Features.Orders;

namespace Week4.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        public DbSet<Order> Orders { get; set; }
    }
}
