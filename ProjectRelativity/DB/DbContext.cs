using Microsoft.EntityFrameworkCore;
using ProjectRelativity.DB.Entities;

namespace ProjectRelativity.DB;

public class MyDbContext: DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}