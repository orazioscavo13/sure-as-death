using System;
using Hangfire;
using Hangfire.PostgreSql;
using SureAsDeath.App.Hangfire.Jobs;
using SureAsDeath.App.Hangfire.Jobs.Interfaces;

namespace SureAsDeath.App.Hangfire
{
    public static class HangfireServices
    {
        public static void AddHangfireServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire((sp, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(config.GetConnectionString("HangfireDatabase")));

            services.AddHangfireServer();

            services.AddScoped<IRetrieveDataJob, RetrieveDataJob>();
        }
    }
}

