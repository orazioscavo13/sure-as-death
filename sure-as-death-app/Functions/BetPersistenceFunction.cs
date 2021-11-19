using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SureAsDeath.App.Common.Models.Configurations;
using SureAsDeath.App.Common.Models.Errors;
using SureAsDeath.App.Common.Models.Requests.EventGrid.Events;
using SureAsDeath.App.Common.Models.TableEntities;
using SureAsDeath.App.Common.Services.AzureQueueStorage;
using SureAsDeath.App.Common.Services.AzureTableStorage;
using SureAsDeath.App.Common.Services.AzureTableStorage.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace sure_as_death_app.Functions
{
    [StorageAccount("StorageConnectionAppSetting")]
    public class BetPersistenceFunction
    {
        private readonly ILogger<BetPersistenceFunction> logger;
        private readonly GlobalConfigurations globalConfigurations;
        private readonly IQueueService queueService;
        private readonly ITableStorageService tableStorage;
        private readonly string errorQueueName;

        public BetPersistenceFunction(
            IQueueService queueService,
            GlobalConfigurations globalConfigurations,
            ITableStorageService tableStorage,
            ILogger<BetPersistenceFunction> logger
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            this.globalConfigurations = globalConfigurations ?? throw new ArgumentNullException(nameof(globalConfigurations));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));

            errorQueueName = $"{globalConfigurations.ErrorPersistenceQueueNamePrefix}{nameof(BetPersistenceFunction).ToLower()}{globalConfigurations.ErrorPersistenceQueueNameSuffix}";

            queueService.AddQueueClients(new[]
            {
                errorQueueName
            });
        }

        [FunctionName(nameof(BetPersistenceFunction))]
        public async Task BetPersistence(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log,
            CancellationToken cToken
            )
        {
            try
            {
                logger.LogInformation($"[{DateTime.Now}][{nameof(BetPersistenceFunction)}] Bet persistance function STARTED");

                BetEvaluationEvent betEvaluationEvent = JsonConvert.DeserializeObject<BetEvaluationEvent>(JsonConvert.SerializeObject(eventGridEvent));

                var tableOperations = tableStorage.CreateTableBatchOperation();

                BetEntity entity = new BetEntity
                {
                    PartitionKey = "Bets",
                    RowKey = Guid.NewGuid().ToString(),
                    Pippo = betEvaluationEvent.Data.Id.ToString()
                };

                tableOperations = tableStorage.AddBatchOperation(tableOperations, entity, OperationType.INSERT);

                await tableStorage.ExecuteTransaction<BetEntity>(tableOperations, cToken);

                logger.LogInformation($"[{DateTime.Now}][{nameof(BetPersistenceFunction)}] Data request CREATED");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[{DateTime.Now}][{nameof(BetPersistenceFunction)}] Exception : {ex.Message}");

                await queueService.PersistErrorAsync(errorQueueName, new QueueError
                {
                    OccuredAt = DateTime.UtcNow,
                    FunctionName = nameof(BetPersistenceFunction),
                    Exception = ex,
                    InputData = eventGridEvent
                });
            }
        }
    }
}
