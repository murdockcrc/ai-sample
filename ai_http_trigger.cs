using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Hosting;
using ai_sample;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(MyStartUp))]
namespace ai_sample
{
    public static class ai_http_trigger
    {
        [FunctionName("ai_http_trigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    internal class MyTelemetryInitializer : ITelemetryInitializer
    {
       IHttpContextAccessor httpContextAccessor;

       public MyTelemetryInitializer(IHttpContextAccessor context)
       {
           // Execution never reaches here
           this.httpContextAccessor = context;
       }
       public void Initialize(ITelemetry telemetry)
       {
           // Execution never reaches here
           telemetry.Context.Operation.Id =
               httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Request-Id") ?
               httpContextAccessor.HttpContext.Request.Headers["Request-Id"].ToString() :
               Guid.NewGuid().ToString();
       }
    }

    public class MyStartUp : IWebJobsStartup
    {
       public void Configure(IWebJobsBuilder builder)
       {
           builder.Services.AddSingleton<ITelemetryInitializer, MyTelemetryInitializer>();
       }
    }
}
