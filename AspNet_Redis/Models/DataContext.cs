using AspNet_Redis.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet_Redis.Context.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }
    }
}
