using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ProjectRelativity;

public static class ItemsList
{
    [FunctionName("ItemsList")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("sqldb_connection"));
        await connection.OpenAsync();
        var list = new List<Item>();
        await using var command = new SqlCommand("SELECT * FROM items", connection);
        var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            list.Add(new Item(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return new OkObjectResult(list);
    }
}