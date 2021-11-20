using System;
using System.Diagnostics.CodeAnalysis;
using Hangfire.Dashboard;

namespace SureAsDeath.App.Authorizations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RemoteAuthorization : Attribute, IDashboardAuthorizationFilter
    {
        public const string API_KEY_HEADER_NAME = "X-HF-KEY";

        public RemoteAuthorization()
        {
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            if (!context.GetHttpContext().Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var apiKey))
            {
                return false;
            }

            var configuration = context.GetHttpContext().RequestServices.GetRequiredService<IConfiguration>();
            var validKey = configuration.GetValue<string>("Hangfire:RemoteAuthKey");

            if (validKey == apiKey)
            {
                return true;
            }

            return false;
        }
    }
}

