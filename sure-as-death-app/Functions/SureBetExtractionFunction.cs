using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SureAsDeath.App.Common.Models.Configurations;
using SureAsDeath.App.Common.Models.Errors;
using SureAsDeath.App.Common.Models.Requests.Queue.Data;
using SureAsDeath.App.Common.Models.TableEntities;
using SureAsDeath.App.Common.Services.AzureQueueStorage;
using SureAsDeath.App.Common.Services.AzureTableStorage;
using SureAsDeath.App.Common.Services.AzureTableStorage.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SureAsDeath.App.Functions
{
    [StorageAccount("StorageConnectionAppSetting")]
    public class SureBetExtractionFunction
    {
        private readonly ILogger<SureBetExtractionFunction> logger;
        private readonly IQueueService queueService;
        private readonly ITableStorageService tableStorage;
        private readonly GlobalConfigurations globalConfigurations;
        private readonly string errorQueueName;
        public SureBetExtractionFunction(
            ILogger<SureBetExtractionFunction> logger, 
            IQueueService queueService,
            ITableStorageService tableStorage,
            GlobalConfigurations globalConfigurations
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            this.globalConfigurations = globalConfigurations ?? throw new ArgumentNullException(nameof(globalConfigurations));

            errorQueueName = $"{globalConfigurations.ErrorPersistenceQueueNamePrefix}{nameof(SureBetExtractionFunction).ToLower()}{globalConfigurations.ErrorPersistenceQueueNameSuffix}";

            /**Adding queue to persist errors **/
            queueService.AddQueueClients(new[] { errorQueueName });
        }

        [FunctionName(nameof(SureBetExtractionFunction))]
        public async Task SureBetExtraction([QueueTrigger("%Queues:MatchesQueueName%")] string message, CancellationToken cToken)
        {
            try
            {
                logger.LogInformation($"[{DateTime.Now}][{nameof(SureBetExtractionFunction)}] Sure bet extraction Function STARTED. Message: {message}");
                RetrieveData matchRequest = JsonConvert.DeserializeObject<RetrieveData>(message);

                var tableOperations = tableStorage.CreateTableBatchOperation();

                SureBetEntity entity = new SureBetEntity
                {
                    PartitionKey = "SureBets",
                    RowKey = Guid.NewGuid().ToString(),
                    Pippo = matchRequest.Match.Pippo
                };

                tableOperations = tableStorage.AddBatchOperation(tableOperations, entity, OperationType.INSERT);

                await tableStorage.ExecuteTransaction<SureBetEntity>(tableOperations, cToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $", [{DateTime.Now}][{nameof(SureBetExtractionFunction)}] Exception : {ex.Message}");

                await queueService.PersistErrorAsync(errorQueueName, new QueueError
                {
                    OccuredAt = DateTime.UtcNow,
                    FunctionName = nameof(SureBetExtractionFunction),
                    Exception = ex,
                    InputData = message
                });
            }
        }
    }
}
