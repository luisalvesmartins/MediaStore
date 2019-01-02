using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class TagValues : TableEntity
    {
        public const string tableContainerName = "TagValues";

        public string TagType { get; set; }
        public string TagValue { get; set; }

        public async Task<bool> SaveAsync(CloudTable tableContainer)
        {
            this.TagType = this.TagType.ToUpper();
            this.TagValue = this.TagValue.ToUpper();
            this.PartitionKey = this.TagType;
            this.RowKey = this.TagValue;
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(this));
            return true;
        }
        public static async Task<List<TagValues>> LoadAllAsync(CloudTable tableContainer)
        {
            string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan,"");

            return await Azure.GetList<TagValues>(tableContainer, query);
        }

    }

}
