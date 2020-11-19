using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrderImport
{
    internal class OrderImportContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        
        public DbSet<Order> Orders { get; set; }

        public OrderImportContext(DbContextOptions<OrderImportContext> options) : base(options) { }
    }

    internal class OrderImportContextFactory : IDesignTimeDbContextFactory<OrderImportContext>
    {
        public OrderImportContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            
            DbContextOptionsBuilder<OrderImportContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

            return new OrderImportContext(optionsBuilder.Options);
        }
    }
}