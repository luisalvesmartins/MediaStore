using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class TagList : TableEntity
    {
        public const string tableContainerName = "TagList";

        public string Tag { get; set; }

        public async Task<bool> SaveAsync(CloudTable tableContainer)
        {
            this.PartitionKey = "TAG";
            this.RowKey = this.Tag;
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(this));
            return true;
        }
        public static async Task<TagList> LoadAsync(CloudTable tableContainer, string Tag)
        {
            string query =
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "TAG"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Tag)
            );

            return await Azure.Get<TagList>(tableContainer, query);
        }
    }

}
