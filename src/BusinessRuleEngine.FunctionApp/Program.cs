using BusinessRuleEngine.FunctionApp.Models;
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
        services.AddOptions<BusinessRulesEngineOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("Bre").Bind(settings);
            });
    })
    .Build();
host.Run();
