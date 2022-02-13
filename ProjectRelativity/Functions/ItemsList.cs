using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectRelativity.DB;

namespace ProjectRelativity.Functions;

public class ItemsList
{
    private readonly MyDbContext _dbContext;

    public ItemsList(MyDbContext context)
    {
        _dbContext = context;
    }
    
    [FunctionName("ItemsList")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        if (!await _dbContext.Items.AnyAsync())
        {
            return new NotFoundResult();
        }
        return new OkObjectResult(await _dbContext.Items.ToListAsync());
    }
}