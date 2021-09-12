using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SureAsDeath.App.Common.Helpers;
using SureAsDeath.App.Common.Models.Configurations;
using SureAsDeath.App.Common.Models.Enumerators;
using SureAsDeath.App.Common.Models.Errors;
using SureAsDeath.App.Common.Models.Requests.Queue.Data;
using SureAsDeath.App.Common.Services.AzureQueueStorage;
using SureAsDeath.Core.Models;

namespace SureAsDeath.App.Functions
{
    [StorageAccount("StorageConnectionAppSetting")]
    public class DataRetrieveFunction
    {
        private readonly ILogger<DataRetrieveFunction> logger;
        private readonly GlobalConfigurations globalConfigurations;
        private readonly IQueueService queueService;
        private readonly string errorQueueName;

        public DataRetrieveFunction(
            IQueueService queueService, 
            GlobalConfigurations globalConfigurations, 
            ILogger<DataRetrieveFunction> logger
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.globalConfigurations = globalConfigurations ?? throw new ArgumentNullException(nameof(globalConfigurations));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));

            errorQueueName = $"{globalConfigurations.ErrorPersistenceQueueNamePrefix}{nameof(DataRetrieveFunction).ToLower()}{globalConfigurations.ErrorPersistenceQueueNameSuffix}";

            queueService.AddQueueClients(new[] 
            { 
                globalConfigurations.MatchesQueueName,
                errorQueueName
            });
        }

        [FunctionName(nameof(DataRetrieveFunction))]
        public async Task RetrieveData([TimerTrigger("%CronTrigger:RetrieveData%")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                logger.LogInformation($"[{DateTime.Now}][{nameof(DataRetrieveFunction)}] Data retrieve function STARTED");

                Match match = new Match
                {
                    Pippo = "Pluto"
                };

                await queueService.AddMessageAsync(
                    $"{globalConfigurations.MatchesQueueName}",
                    Utils.EncodeToBase64String(
                        JsonConvert.SerializeObject(
                            new RetrieveData
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.UtcNow,
                                MessageType = MessageType.Standard,
                                Match = match
                            }
                            )
                        )
                    );

                logger.LogInformation($"[{DateTime.Now}][{nameof(DataRetrieveFunction)}] Data request CREATED");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[{DateTime.Now}][{nameof(DataRetrieveFunction)}] Exception : {ex.Message}");

                await queueService.PersistErrorAsync(errorQueueName, new QueueError
                {
                    OccuredAt = DateTime.UtcNow,
                    FunctionName = nameof(DataRetrieveFunction),
                    Exception = ex,
                    InputData = myTimer
                });
            }
        }
    }
}
