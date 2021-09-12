using System;

namespace SureAsDeath.App.Common.Models.Configurations
{
    public class GlobalConfigurations
    {
        public string ErrorPersistenceQueueNameSuffix { get; set; }
        public string ErrorPersistenceQueueNamePrefix { get; set; }
        public string MatchesQueueName { get; set; }
    }
}
