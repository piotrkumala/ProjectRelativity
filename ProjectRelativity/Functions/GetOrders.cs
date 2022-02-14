using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectRelativity.DB;

namespace ProjectRelativity.Functions;

public class GetOrders
{
    private readonly MyDbContext _dbContext;

    public GetOrders(MyDbContext context)
    {
        _dbContext = context;
    }

    [FunctionName("GetOrders")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        if (!req.Query.ContainsKey("UserId"))
        {
            return new BadRequestObjectResult("No userId specified");
        }

        if (!await _dbContext.Orders.AnyAsync(x => x.UserId == req.Query["UserId"].ToString()))
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(await _dbContext.Orders.Where(x => x.UserId == req.Query["UserId"].ToString())
            .Include(x => x.OrderItems).ThenInclude(x => x.Item).ToListAsync());
    }
}