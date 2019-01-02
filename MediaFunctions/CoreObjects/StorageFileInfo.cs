using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class StorageFileInfo : TableEntity
    {
        public const string tableContainerName = "media";
        public const string NoThumbnails = ".MOV.AVI.DIVX.MPEG.MPG.MP4.MP3.MTS.HEIC.M4V.AAE.";
        public const string NoUpload = ".DB.DAT.TMP.DOC.PUB.";

        public static bool canCreateThumbnail(string ext)
        {
            return (StorageFileInfo.NoThumbnails.IndexOf(ext.ToUpper() + ".") < 0);
        }

        public string Directory { get; set; }
        public DateTime CreationTime { get; set; }
        public string Extension { get; set; }
        public long Length { get; set; }
        public string Vision { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Tags { get; set; }
        public string EXIF { get; set; }

        public static async Task<CloudTable> GetTableContainerAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableContainer = tableClient.GetTableReference(tableContainerName);
            await tableContainer.CreateIfNotExistsAsync();
            return tableContainer;
        }

        public async Task<bool> SaveAsync(CloudTable tableContainer)
        {
            this.PartitionKey = Id;
            this.RowKey = "Media";
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(this));
            return true;
        }
        public static async Task<bool> DeleteByIdAsync(CloudTable tableContainer,string id)
        {
            StorageFileInfo SFI=await LoadAsync(tableContainer, id);

            TableOperation delete = TableOperation.Delete(SFI);
            await tableContainer.ExecuteAsync(delete);

            return true;
        }
        public static async Task<StorageFileInfo> LoadAsync(string connectionString, string id)
        {
            CloudTable tableSFI = await StorageFileInfo.GetTableContainerAsync(connectionString);
            return await StorageFileInfo.LoadAsync(tableSFI, id);
        }

        public static async Task<StorageFileInfo> LoadAsync(CloudTable tableContainer, string id)
        {
            TableOperation retrieve = TableOperation.Retrieve<StorageFileInfo>(id, "Media");
            TableResult TR = await tableContainer.ExecuteAsync(retrieve);
            StorageFileInfo returnedObject = (StorageFileInfo)TR.Result;

            return returnedObject;
        }
        public static async Task<bool> RenameDirectory(CloudTable tableContainer, string oldDir, string NewDir)
        {
            string query = TableQuery.GenerateFilterCondition("Directory", QueryComparisons.Equal, oldDir);

            var media = await Azure.GetList<StorageFileInfo>(tableContainer, query);

            foreach (StorageFileInfo item in media)
            {
                item.Directory = NewDir;
                await item.SaveAsync(tableContainer);
            }

            return true;
        }
 
        public static async Task<List<StorageFileInfo>> ListAsync(CloudTable tableContainer)
        {
            string query =TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Media");

            return await Azure.GetList<StorageFileInfo>(tableContainer, query);
        }
   
        public static async Task<List<StorageFileInfo>> getFilesFromDirectoryAsync(CloudTable tableContainer, string directory)
        {
            string query=
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Media"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Directory", QueryComparisons.Equal, directory)
                );

            var tas = await Azure.GetList<StorageFileInfo>(tableContainer, query);
            return tas;
        }

    }
}
