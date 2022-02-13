using Microsoft.EntityFrameworkCore;

namespace ProjectRelativity.DB;

public class MyDbContext: DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
}