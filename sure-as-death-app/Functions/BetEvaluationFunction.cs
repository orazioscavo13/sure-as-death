using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using SureAsDeath.App.Common.Constants;
using SureAsDeath.App.Common.Models.Configurations;
using SureAsDeath.App.Common.Models.Enumerators;
using SureAsDeath.App.Common.Models.Errors;
using SureAsDeath.App.Common.Models.Requests.EventGrid.Data;
using SureAsDeath.App.Common.Services.AzureQueueStorage;
using System;
using System.Threading.Tasks;

namespace sure_as_death_app.Functions
{
    [StorageAccount("StorageConnectionAppSetting")]
    public class BetEvaluationFunction
    {
        private readonly ILogger<BetEvaluationFunction> logger;
        private readonly GlobalConfigurations globalConfigurations;
        private readonly IQueueService queueService;
        private readonly string errorQueueName;

        public BetEvaluationFunction(
            IQueueService queueService,
            GlobalConfigurations globalConfigurations,
            ILogger<BetEvaluationFunction> logger
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.globalConfigurations = globalConfigurations ?? throw new ArgumentNullException(nameof(globalConfigurations));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));

            errorQueueName = $"{globalConfigurations.ErrorPersistenceQueueNamePrefix}{nameof(BetEvaluationFunction).ToLower()}{globalConfigurations.ErrorPersistenceQueueNameSuffix}";

            queueService.AddQueueClients(new[]
            {
                errorQueueName
            });
        }

        [FunctionName(nameof(BetEvaluationFunction))]
        public async Task BetEvaluation(
            [TimerTrigger("%CronTrigger:BetEvaluation%")] TimerInfo myTimer, 
            [EventGrid(TopicEndpointUri = "Topic:BetEvaluation:Endpoint", TopicKeySetting = "Topic:BetEvaluation:Key")] IAsyncCollector<EventGridEvent> outputEvents,
            ILogger log
            //Tabella SureBet(lettura input)
            )
        {
            try
            {
                logger.LogInformation($"[{DateTime.Now}][{nameof(BetEvaluationFunction)}] Bet evaluation function STARTED");

                await outputEvents.AddAsync(new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = $"{EventType.NewBet}",
                    Subject = $"{EventSubject.None}",
                    EventTime = DateTime.UtcNow,
                    DataVersion = EventGridConstants.DataVersion,
                    Data = new BetEvaluation
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow,
                        MessageType = MessageType.Standard
                    }
                });

                logger.LogInformation($"[{DateTime.Now}][{nameof(BetEvaluationFunction)}] Data request CREATED");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[{DateTime.Now}][{nameof(BetEvaluationFunction)}] Exception : {ex.Message}");

                await queueService.PersistErrorAsync(errorQueueName, new QueueError
                {
                    OccuredAt = DateTime.UtcNow,
                    FunctionName = nameof(BetEvaluationFunction),
                    Exception = ex,
                    InputData = myTimer
                });
            }
        }
    }
}
