using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProjectRelativity.ClientObjects;
using ProjectRelativity.DB;
using ProjectRelativity.Functions;
using Xunit;

namespace ProjectRelativityTests;

public class PlaceOrderTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<MyDbContext> _contextOptions;
    private readonly Mock<ILogger> _mockLogger = new();

    public PlaceOrderTests()
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
    public async Task ShouldReturnUnprocessableEntryWhenNoItemsInOrder()
    {
        var json = JsonConvert.SerializeObject(new Order("User", new List<OrderItem>()));
        var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
        await memoryStream.FlushAsync();
        memoryStream.Position = 0;
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Body).Returns(memoryStream);
        await using var context = CreateContext();
        var placeOrder = new PlaceOrder(context);

        var result = await placeOrder.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(UnprocessableEntityObjectResult));
    }

    [Fact]
    public async Task ShouldReturnUnprocessableEntityWhenInvalidItemOrdered()
    {
        var json = JsonConvert.SerializeObject(new Order("User",
            new List<OrderItem> {new OrderItem(10, new Item(1, "Item", 20.2))}));
        var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
        await memoryStream.FlushAsync();
        memoryStream.Position = 0;
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Body).Returns(memoryStream);
        await using var context = CreateContext();
        var placeOrder = new PlaceOrder(context);

        var result = await placeOrder.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(UnprocessableEntityObjectResult));
    }

    [Fact]
    public async Task ShouldSaveProperOrderToDatabase()
    {
        var json = JsonConvert.SerializeObject(new Order("User",
            new List<OrderItem> {new OrderItem(10, new Item(1, "Item", 20.2))}));
        var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(json));
        await memoryStream.FlushAsync();
        memoryStream.Position = 0;
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Body).Returns(memoryStream);
        await using var context = CreateContext();
        await context.Items.AddAsync(new ProjectRelativity.DB.Entities.Item(1, "Item", 20.2));
        await context.SaveChangesAsync();
        var placeOrder = new PlaceOrder(context);

        var result = await placeOrder.RunAsync(mockRequest.Object, _mockLogger.Object);

        result.Should().NotBeNull().And.BeOfType(typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var entity = okResult!.Value as ProjectRelativity.DB.Entities.Order;
        entity!.UserId.Should().BeEquivalentTo("User");
        entity!.OrderItems.Should().HaveCount(1).And.ContainEquivalentOf(new ProjectRelativity.DB.Entities.OrderItem
            {Amount = 10, Id = 1, Item = new ProjectRelativity.DB.Entities.Item(1, "Item", 20.2), ItemId = 1});
    }
}