using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using Lab3.Models;

namespace Lab3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        }
}
