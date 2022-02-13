using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using ProjectRelativity.DB;
using ProjectRelativity.DB.Entities;
using ProjectRelativity.Functions;
using Xunit;

namespace ProjectRelativityTests;

public class GetOrdersTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<MyDbContext> _contextOptions;
    private readonly Mock<ILogger> _mockLogger = new();

    public GetOrdersTests()
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

    MyDbContext CreateContext() => new(_contextOptions);

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task ShouldReturnBadRequestWhenNoQueryParameter()
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Query).Returns(new QueryCollection());
        await using var context = CreateContext();
        var getOrders = new GetOrders(context);

        var result = await getOrders.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenNoOrdersInDb()
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest
            .Setup(x => x.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> {{"UserId", "User"}}));
        await using var context = CreateContext();
        var getOrders = new GetOrders(context);

        var result = await getOrders.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(NotFoundResult));
    }

    [Fact]
    public async Task ShouldReturnUsersOrders()
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest
            .Setup(x => x.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues> {{"UserId", "User"}}));
        await using var context = CreateContext();
        var item = new Item(1, "Item", 1.2);
        await context.Items.AddRangeAsync(new List<Item> {item});
        var entity = new Order
        {
            Id = 1, UserId = "User",
            OrderItems = new List<OrderItem> {new OrderItem {Amount = 10, Id = 1, Item = item, ItemId = item.Id}}
        };
        await context.OrderItems.AddRangeAsync(entity.OrderItems);
        await context.Orders.AddAsync(entity);
        await context.SaveChangesAsync();
        var getOrders = new GetOrders(context);

        var result = await getOrders.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(OkObjectResult));
        var okResult = result as OkObjectResult;

        okResult!.Value.Should().BeEquivalentTo(new List<Order>{entity});
    }
}