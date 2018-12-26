using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoUploader
{
    public class Tags : TableEntity
    {
        //public const string tableContainerName = "Tag";
        //public const string tableContainerNameReverse = "TagIndex";

        //ID, Name string, PersonId Guid
        public string Id { get; set; }
        public string Tag { get; set; }
        public string TagType { get; set; }

        public static CloudTable GetTableContainer(CloudStorageAccount storageAccount,string tableContainerName)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableContainer = tableClient.GetTableReference(tableContainerName);
            tableContainer.CreateIfNotExists();
            return tableContainer;
        }

        public bool Save(CloudTable tableContainer, CloudTable tableContainerReverse)
        {
            this.PartitionKey = this.Id;
            this.RowKey = this.TagType + ":" + this.Tag.Replace("\\","|");
            tableContainer.Execute(TableOperation.InsertOrReplace(this));
            //CREATE REVERSE
            MediaTagReverse MTR = new MediaTagReverse();
            MTR.Save(tableContainerReverse, this.Id, this.TagType + ":" + this.Tag);
            return true;
        }


    }
    public class MediaTagReverse : TableEntity
    {
        public bool Save(CloudTable tableContainer, string Id, string Tag)
        {
            this.PartitionKey = Tag.Replace("\\","|");
            this.RowKey = Id;
            tableContainer.Execute(TableOperation.InsertOrReplace(this));
            return true;
        }
    }
}
