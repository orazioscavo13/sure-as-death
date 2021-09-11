using Microsoft.Azure.Cosmos.Table;
using SureAsDeath.App.Common.Services.AzureTableStorage.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SureAsDeath.App.Common.Services.AzureTableStorage
{
    public interface ITableStorageService
    {
        Task<List<T>> GetEntities<T>(string partitionKey, CancellationToken cToken) where T : TableEntity, new();
        Task<T> GetSingleAsync<T>(string partitionKey, string rowKey, CancellationToken cToken) where T : TableEntity, new();
        Task<T> UpdateAsync<T>(T tableEntityData, CancellationToken cToken) where T : TableEntity, new();
        Task<T> AddAsync<T>(T tableEntityData, CancellationToken cToken) where T : TableEntity, new();
        Task<IEnumerable<T>> UpdateAsyncBulk<T>(List<T> tableEntityData, CancellationToken cToken) where T : TableEntity, new();
        Task<IEnumerable<T>> DeleteBulk<T>(List<T> tableEntityData, CancellationToken cToken) where T : TableEntity, new();
        TableBatchOperation CreateTableBatchOperation();
        TableBatchOperation AddBatchOperation<T>(TableBatchOperation tableOperations, T tableEntity, OperationType operation) where T : TableEntity, new();
        Task<IEnumerable<T>> ExecuteTransaction<T>(TableBatchOperation tableOperations, CancellationToken cToken) where T : TableEntity, new();
    }
}
