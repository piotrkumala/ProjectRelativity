using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectRelativity.DB;

namespace ProjectRelativity;

[assembly: FunctionsStartup(typeof(ProjectRelativity.Startup))]
public class Startup: FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable("sqldb_connection");
        builder.Services.AddDbContext<MyDbContext>(options => options.UseSqlServer(connectionString!));
    }
}