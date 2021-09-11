using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues.Models;
using SureAsDeath.App.Common.Models.Errors;

namespace SureAsDeath.App.Common.Services.AzureQueueStorage
{
    public interface IQueueService
    {
        void AddQueueClients(string[] queuesNames);
        bool IsQueueClientInitialized(string queueName);
        Task<SendReceipt> AddMessageAsync(string queueName, string message);
        Task<QueueMessage> GetMessageAsync(string queueName, TimeSpan? visibilityTimeout = null);
        Task<QueueMessage[]> GetMessagesAsync(string queueName, int maxMessages, TimeSpan? visibilityTimeout = null);
        Task<UpdateReceipt> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string updatedContents, TimeSpan visibilityTimeout = default);
        Task<Response> DeleteMessagesAsync(string queueName, string messageId, string popReceipt);
        Task PersistErrorAsync(string queueName, QueueError queueError);
        Task PersistErrorAndDeleteMessageFromSourceQueueAsync(string queueName, QueueError queueError, string sourceQueueName);
    }
}
