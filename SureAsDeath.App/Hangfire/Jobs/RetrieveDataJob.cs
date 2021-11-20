using System;
using Hangfire;
using SureAsDeath.App.Hangfire.Jobs.Interfaces;

namespace SureAsDeath.App.Hangfire.Jobs
{
    public class RetrieveDataJob : IRetrieveDataJob
    {
        private readonly ILogger<RetrieveDataJob> logger;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IConfiguration configuration;

        public RetrieveDataJob(
            ILogger<RetrieveDataJob> logger,
            IRecurringJobManager recurringJobManager,
            IConfiguration configuration
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void SyncData(CancellationToken cancellationToken)
        {
            logger.LogInformation($"[{DateTime.Now}] Sync Data JOB SCHEDULED");

            recurringJobManager.AddOrUpdate(
                    nameof(RetrieveDataJob),
                    () => RetrieveFromProvider(cancellationToken),
                    "* * * * * *");
        }

        public async Task RetrieveFromProvider(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogError("TRIGGERED");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[{DateTime.Now}] Exception : {ex.Message}");
            }
        }
    }
}

