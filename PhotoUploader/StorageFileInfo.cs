using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoUploader
{
    public class StorageFileInfo : TableEntity
    {
        public const string NoThumbnails = ".MOV.AVI.DIVX.MPEG.MPG.MP4.MP3.MTS.HEIC.M4V.AAE.";

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
        public string EXIF { get; set; }

        public static CloudTable GetTableContainer(CloudStorageAccount storageAccount, string tableContainerName)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableContainer = tableClient.GetTableReference(tableContainerName);
            tableContainer.CreateIfNotExists();
            return tableContainer;
        }
        public bool Save(CloudTable tableContainer)
        {
            this.PartitionKey = Id;
            this.RowKey = "Media";
            tableContainer.Execute(TableOperation.InsertOrReplace(this));
            return true;
        }
        public static StorageFileInfo Load(CloudTable tableContainer,string partitionKey)
        {
            TableOperation retrieve = TableOperation.Retrieve<StorageFileInfo>(partitionKey, "Media");
            StorageFileInfo returnedObject = (StorageFileInfo)tableContainer.Execute(retrieve).Result;

            return returnedObject;
        }
        public static bool FindEqual(CloudTable tableContainer, string fileName, long fileSize)
        {
            string filterA = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, fileName),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("Length", QueryComparisons.Equal, fileSize)
                );

            TableQuery<StorageFileInfo> itemStockQuery = new TableQuery<StorageFileInfo>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Media"),
                    TableOperators.And,
                    filterA
                )
                    );

            var rawMtlStock = tableContainer.ExecuteQuery(itemStockQuery);
            if (rawMtlStock.Any())
            {
                return true;
            }

            return false;
        }
        public static List<StorageFileInfo> List(CloudTable tableContainer)
        {
            List<StorageFileInfo> listSFI = new List<StorageFileInfo>();

            TableQuery<StorageFileInfo> itemStockQuery = new TableQuery<StorageFileInfo>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Media")
                    );

            var rawMtlStock = tableContainer.ExecuteQuery(itemStockQuery).ToList();
            listSFI.AddRange(rawMtlStock);

            return listSFI;
        }
    
  
    }

        public class PathInfo
        {
            public string Title { get; set; }
            public int Items { get; set; }
        }

}
