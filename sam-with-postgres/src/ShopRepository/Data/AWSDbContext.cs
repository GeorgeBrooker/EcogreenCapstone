using Microsoft.EntityFrameworkCore;
using ShopRepository.Models;

namespace ShopRepository.Data;

public class AWSDbContext : DbContext
{
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Stock> Stock { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    
    public AWSDbContext(DbContextOptions<AWSDbContext> options) : base(options) {}
}