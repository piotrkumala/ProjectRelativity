using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjectRelativity.ClientObjects;
using ProjectRelativity.DB;
using OrderItem = ProjectRelativity.DB.Entities.OrderItem;

namespace ProjectRelativity.Functions;

public class PlaceOrder
{
    private readonly MyDbContext _dbContext;

    public PlaceOrder(MyDbContext context)
    {
        _dbContext = context;
    }
    [FunctionName("PlaceOrder")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        var order = JsonConvert.DeserializeObject<Order>(await new StreamReader(req.Body).ReadToEndAsync());
        
        var entity = new DB.Entities.Order {UserId = order.UserId, OrderItems = order.OrderItems.Select(x => new OrderItem
        {
            Amount = x.Amount, Item = _dbContext.Items.First(item => item.Id == x.Item.Id), ItemId = x.Item.Id
        }).ToList()};
        
        await _dbContext.OrderItems.AddRangeAsync(entity.OrderItems);
        await _dbContext.Orders.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        
        log.LogInformation($"Added order with Id={entity.Id}");

        return new OkObjectResult(await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == entity.Id));
    }
}