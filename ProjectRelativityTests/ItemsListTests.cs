using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectRelativity.DB;
using ProjectRelativity.DB.Entities;
using ProjectRelativity.Functions;
using Xunit;

namespace ProjectRelativityTests;

public class ItemsListTests: IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<MyDbContext> _contextOptions;

    public ItemsListTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();    
        
        _contextOptions = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        using var context = new MyDbContext(_contextOptions);

        context.Database.EnsureCreated();

        context.SaveChanges();
    }
    
    MyDbContext CreateContext() => new (_contextOptions);

    public void Dispose() => _connection.Dispose();
    
    [Fact]
    public async Task ShouldReturn404OnEmptyDB()
    {
        var mockLogger = new Mock<ILogger>();
        await using var context = CreateContext();
        var itemsList = new ItemsList(context);
            
        var result = await itemsList.RunAsync(null, mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(NotFoundResult));
    }
    
    [Fact]
    public async Task ShouldReturnAllItems()
    {
        var mockLogger = new Mock<ILogger>();
        var items = new List<Item>
        {
            new(1, "Item1", 1.2),
            new(2, "Item2", 2.45)
        };
        await using var context = CreateContext();
        await context.Items.AddRangeAsync(items);
        await context.SaveChangesAsync();
        var itemsList = new ItemsList(context);
            
        var result = await itemsList.RunAsync(null, mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(items);
    }
    
}