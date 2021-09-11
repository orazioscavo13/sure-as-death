using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using SureAsDeath.App.Common.Models.Errors;
using SureAsDeath.App.Common.Models.Requests.Base;

namespace SureAsDeath.App.Common.Services.AzureQueueStorage
{
    public class QueueService : IQueueService
    {
        private readonly string storageConnectionString;
        private readonly Dictionary<string, QueueClient> queueClients = new Dictionary<string, QueueClient>();

        public QueueService(string storageConnectionString, string[] queuesNames = null)
        {
            this.storageConnectionString = storageConnectionString ?? throw new ArgumentNullException(nameof(storageConnectionString));

            if (queuesNames != null)
                InitializeQueueClients(storageConnectionString, queuesNames);
        }

        public void AddQueueClients(string[] queuesNames)
        {
            InitializeQueueClients(storageConnectionString, queuesNames);
        }

        public bool IsQueueClientInitialized(string queueName)
        {
            return queueClients.ContainsKey(queueName);
        }

        public async Task<SendReceipt> AddMessageAsync(string queueName, string message)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
            {
                return await queueClient.SendMessageAsync(message);
            }

            throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client or does not exist. Provide it to the service constructor");
        }

        public async Task<QueueMessage> GetMessageAsync(string queueName, TimeSpan? visibilityTimeout = null)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
            {
                return await queueClient.ReceiveMessageAsync(visibilityTimeout);
            }

            throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client. Provide it to the service constructor");
        }

        public async Task<QueueMessage[]> GetMessagesAsync(string queueName, int maxMessages, TimeSpan? visibilityTimeout = null)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
            {
                return await queueClient.ReceiveMessagesAsync(maxMessages, visibilityTimeout);
            }

            throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client. Provide it to the service constructor");
        }

        public async Task<UpdateReceipt> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string updatedContents, TimeSpan visibilityTimeout = default)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
            {
                return await queueClient.UpdateMessageAsync(messageId, popReceipt, updatedContents, visibilityTimeout);
            }

            throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client. Provide it to the service constructor");
        }

        public async Task<Response> DeleteMessagesAsync(string queueName, string messageId, string popReceipt)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
            {
                return await queueClient.DeleteMessageAsync(messageId, popReceipt);
            }

            throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client. Provide it to the service constructor");
        }

        public async Task PersistErrorAsync(string queueName, QueueError queueError)
        {
            if (queueClients.TryGetValue(queueName, out QueueClient queueClient) && queueClient.Exists())
                await queueClient.SendMessageAsync(JsonConvert.SerializeObject(queueError));
            else
                throw new ArgumentException($"The queue name '{queueName}' does not have an instance of Queue Client or does not exist. Provide it to the service constructor");
        }
        public async Task PersistErrorAndDeleteMessageFromSourceQueueAsync(string queueName, QueueError queueError, string sourceQueueName)
        {
            await PersistErrorAsync(queueName, queueError);

            var basicQueueMessage = queueError.InputData as QueueMessageBase;

            if (basicQueueMessage != null)
                await DeleteMessagesAsync(sourceQueueName, basicQueueMessage.MessageId, basicQueueMessage.PopReceipt);
            else
                throw new ArgumentException($"The type of 'inputData' parameter is not a QueueMessage");
        }
        private void InitializeQueueClients(string queueConnectionString, string[] queuesNames)
        {
            foreach (var queueName in queuesNames)
            {
                if (!queueClients.ContainsKey(queueName))
                    queueClients.Add(queueName, new QueueClient(queueConnectionString, queueName));
            }
        }
    }
}
