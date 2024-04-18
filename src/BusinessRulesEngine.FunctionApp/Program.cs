using BusinessRulesEngine.FunctionApp.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddOptions<OpenAiOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("OpenAi").Bind(settings);
            });
    })
    .Build();
host.Run();
