using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class Azure
    {
        public static string GetAccountSASToken(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a new access policy for the account.
            SharedAccessAccountPolicy policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.List,
                Services = SharedAccessAccountServices.Blob | SharedAccessAccountServices.File,
                ResourceTypes = SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Protocols = SharedAccessProtocol.HttpsOrHttp
            };

            // Return the SAS token.
            return storageAccount.GetSharedAccessSignature(policy);
        }
        public async static Task<T> Get<T>(CloudTable table, string tableQuery) where T : ITableEntity, new()
        {
            //new T();
            TableQuery<T> employeeQuery = new TableQuery<T>().Where(
                    tableQuery
        ).Take(1);
            var re = new T();
            TableContinuationToken continuationToken = null;
            do
            {
                var employees = await table.ExecuteQuerySegmentedAsync(employeeQuery, continuationToken);

                re = employees.FirstOrDefault();
                continuationToken = employees.ContinuationToken;
            } while (continuationToken != null);
            return re;
        }

        public async static Task<List<T>> GetList<T>(CloudTable table, string tableQuery) where T : ITableEntity, new()
        {
            //new T();
            TableQuery<T> employeeQuery = new TableQuery<T>().Where(
                    tableQuery
        );
            var rel = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                var employees = await table.ExecuteQuerySegmentedAsync(employeeQuery, continuationToken);

                rel.AddRange(employees.ToList());
                continuationToken = employees.ContinuationToken;
            } while (continuationToken != null);
            return rel;
        }
        public async static Task<List<T>> GetList<T>(CloudTable table, TableQuery<T> TQ) where T : ITableEntity, new()
        {
            var rel = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                var employees = await table.ExecuteQuerySegmentedAsync(TQ, continuationToken);
                rel.AddRange(employees.ToList());
                continuationToken = employees.ContinuationToken;
            } while (continuationToken != null);
            return rel;
        }

        public static async Task<CloudTable> GetTableContainerAsync(string connectionString, string tableContainerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            return await GetTableContainerAsync(storageAccount,tableContainerName);
        }

        public static async Task<CloudTable> GetTableContainerAsync(CloudStorageAccount storageAccount, string tableContainerName)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableContainer = tableClient.GetTableReference(tableContainerName);
            await tableContainer.CreateIfNotExistsAsync();
            return tableContainer;
        }

    }
}
