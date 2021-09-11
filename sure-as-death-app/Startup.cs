using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SureAsDeath.App;
using SureAsDeath.App.Common.Models.Configurations;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SureAsDeath.App
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            //Get the function app directory
            var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;
            var currentDirectory = executionContextOptions.AppDirectory;


            // Load the custom configuration files
            var config = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile($"Configurations/{Environment.GetEnvironmentVariable("Environment")}.global.json", false)
                .AddEnvironmentVariables()
                .Build();

            // Need to register the above setup configuration provider which includes the custom configuration files
            services.AddSingleton<IConfiguration>(config);

            services.AddOptions<GlobalConfigurations>().Configure<IConfiguration>((customSetting, configuration) => { configuration.GetSection("GlobalConfigurations").Bind(customSetting); });

            services.AddScoped(provider =>
            {
                var options = provider.GetRequiredService<IOptions<GlobalConfigurations>>();
                return options.Value;
            });

            services.AddScoped<IQueueService>(sp => new QueueService(Environment.GetEnvironmentVariable("StorageConnectionAppSetting", EnvironmentVariableTarget.Process)));

            services.AddScoped<ITableStorageService>(sp => new TableStorageService(
                Environment.GetEnvironmentVariable("StorageConnectionAppSetting", EnvironmentVariableTarget.Process),
                Environment.GetEnvironmentVariable("TableReference", EnvironmentVariableTarget.Process),
                Int32.Parse(Environment.GetEnvironmentVariable("MaxBatchOperations", EnvironmentVariableTarget.Process))
                )
            );

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
