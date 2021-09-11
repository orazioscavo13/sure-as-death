using SureAsDeath.App.Common.Services.AzureTableStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace SureAsDeath.App.Common.Services.AzureTableStorage
{
    public class TableStorageService : ITableStorageService
    {
        private readonly CloudTable table;
        private readonly int maxBatchOperations;

        public TableStorageService(string storageConnectionString, string referenceTable, int maxBatchOperations)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(referenceTable);
            this.maxBatchOperations = maxBatchOperations;
        }

        public async Task<List<T>> GetEntities<T>(string partitionKey, CancellationToken cToken) where T : TableEntity, new()
        {
            var exQuery = new TableQuery<T>();

            if (partitionKey != null)
            {
                exQuery = exQuery.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }

            var result = new List<T>();
            TableContinuationToken tableContinuationToken = null;

            do
            {
                var queryResponse = await table.ExecuteQuerySegmentedAsync(exQuery, tableContinuationToken, cToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                result.AddRange(queryResponse.Results);
            } while (tableContinuationToken != null);

            return result.Select(ent => (T)ent).ToList();
        }

        public async Task<T> GetSingleAsync<T>(string partitionKey, string rowKey, CancellationToken cToken) where T : TableEntity, new()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await table.ExecuteAsync(retrieveOperation, cToken);
            return (T)tableResult.Result;
        }

        public async Task<T> UpdateAsync<T>(T tableEntityData, CancellationToken cToken) where T : TableEntity, new()
        {
            var updateOperation = TableOperation.InsertOrReplace(tableEntityData);
            var tableResult = await table.ExecuteAsync(updateOperation);
            return (T)tableResult.Result;
        }

        public async Task<IEnumerable<T>> UpdateAsyncBulk<T>(List<T> tableEntityData, CancellationToken cToken) where T : TableEntity, new()
        {
            var bulkOperationObj = new TableBatchOperation();
            foreach (var entity in tableEntityData)
            {
                bulkOperationObj.InsertOrReplace(entity);
            }
            return await ExecuteTransaction<T>(bulkOperationObj, cToken);
        }

        public async Task<T> AddAsync<T>(T tableEntityData, CancellationToken cToken) where T : TableEntity, new()
        {
            var retrieveOperation = TableOperation.Insert(tableEntityData);
            var tableResult = await table.ExecuteAsync(retrieveOperation, cToken);
            return (T)tableResult.Result;
        }

        public async Task<IEnumerable<T>> DeleteBulk<T>(List<T> tableEntityData, CancellationToken cToken) where T : TableEntity, new()
        {
            // Create the batch operation.
            TableBatchOperation batchDeleteOperation = new TableBatchOperation();

            foreach (var row in tableEntityData)
            {
                batchDeleteOperation.Delete(row);
            }

            // Execute the batch operation.
            return await ExecuteTransaction<T>(batchDeleteOperation, cToken);
        }

        public TableBatchOperation CreateTableBatchOperation()
        {
            return new TableBatchOperation();
        }

        public TableBatchOperation AddBatchOperation<T>(TableBatchOperation tableOperations, T tableEntity, OperationType operation) where T : TableEntity, new()
        {
            switch (operation)
            {
                case OperationType.INSERT:
                    tableOperations.Insert(tableEntity);
                    break;
                case OperationType.UPDATE:
                    tableOperations.InsertOrReplace(tableEntity);
                    break;
                case OperationType.DELETE:
                    tableOperations.Delete(tableEntity);
                    break;
                default:
                    throw new InvalidOperationException("Invalid Table Operation");
            }

            return tableOperations;
        }

        public async Task<IEnumerable<T>> ExecuteTransaction<T>(TableBatchOperation tableOperations, CancellationToken cToken) where T : TableEntity, new()
        {
            var chunks = GetChunks(tableOperations);

            var response = new List<T>();

            foreach (var chunk in chunks)
            {
                var tableResult = await table.ExecuteBatchAsync(chunk);
                response.AddRange(tableResult.Select(r => (T)r.Result));
            }

            return response;
        }

        private IEnumerable<TableBatchOperation> GetChunks(TableBatchOperation fullSet)
        {
            return fullSet
                .ToList()
                .Select((s, i) => new { Value = s, Index = i })
                .GroupBy(x => x.Index / maxBatchOperations)
                .Select(grp =>
                {
                    var elements = grp.Select(to => to.Value);
                    var newTableOperations = new TableBatchOperation();

                    foreach (var elem in elements)
                    {
                        newTableOperations.Add(elem);
                    }

                    return newTableOperations;
                })
                .ToList();
        }
    }
}
