using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blasor_demo_container;
using Dapr.Client;
using Microsoft.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Trace);

Console.WriteLine($"foo = {builder.Configuration["message"]}");

builder.Services.AddSingleton<DaprClient>(sp => 
{
    try
    {
        return new DaprClientBuilder().Build();
    } 
    catch (Exception ex)
    {
        Console.WriteLine("Error initializing DaprClient:");
        Console.WriteLine(ex.Message);
        throw;
    }
});
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:3500/") });

try 
{
    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Error running the application:");
    Console.WriteLine(ex.Message);
}
