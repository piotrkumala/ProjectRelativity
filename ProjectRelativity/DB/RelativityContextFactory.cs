using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProjectRelativity.DB;

public class RelativityContextFactory: IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("sqldb_connection")!);

            return new MyDbContext(optionsBuilder.Options);
        }
}