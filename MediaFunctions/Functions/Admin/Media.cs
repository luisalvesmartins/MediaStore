using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CoreObjects;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Functions
{
    public static class Media
    {
        /// <summary>
        /// Rename Directory
        /// </summary>
        /// <param name="oldDir"></param>
        /// <param name="newDir"></param>
        /// <returns></returns>
        [FunctionName("admRenameDirectory")]
        public static async Task<IActionResult> admRenameDirectory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");
            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }
            if (!U.isAdmin)
            {
                return new BadRequestObjectResult("No admin rights");
            }

            CloudTable tableSFI = await StorageFileInfo.GetTableContainerAsync(connectionString);

            string oldDir = req.Query["oldDir"];
            string newDir = req.Query["newDir"];
            string id = req.Query["id"];
            if (string.IsNullOrEmpty(id))
            {
                await StorageFileInfo.RenameDirectory(tableSFI, oldDir, newDir);
                return (ActionResult)new OkObjectResult(true);
            }
            else
            {
                StorageFileInfo SFI = await StorageFileInfo.LoadAsync(tableSFI, id);
                if (SFI != null)
                {
                    if (SFI.Directory==oldDir)
                    {
                        SFI.Directory = newDir;
                        await SFI.SaveAsync(tableSFI);
                        return (ActionResult)new OkObjectResult(true);
                    }
                    return new BadRequestObjectResult("Wrong old dir:" + SFI.Directory);
                }
                return new BadRequestObjectResult("Not found:" + id);
            }
        }




        [FunctionName("MediaDelete")]
        public static async Task<IActionResult> MediaDelete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }
            if (!U.isAdmin)
            {
                return new BadRequestObjectResult("No admin rights");
            }

            string id = req.Query["id"];
            log.LogInformation("Media Delete:" + id);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudTable tableTagIndex = await Azure.GetTableContainerAsync(storageAccount, TagIndex.tableContainerName);
            CloudTable tableTag = await Azure.GetTableContainerAsync(storageAccount, MediaTag.tableContainerName);
            CloudTable tableSFI = await Azure.GetTableContainerAsync(storageAccount, StorageFileInfo.tableContainerName);

            CloudBlobContainer blobContainerEXIF = blobClient.GetContainerReference("exif");

            //DELETE FROM 
            await StorageFileInfo.DeleteByIdAsync(tableSFI, id);
            //DELETE FROM TAG
            await TagIndex.DeleteByIdAsync(tableTagIndex, id);
            //DELETE FROM TAG
            await MediaTag.DeleteByIdAsync(tableTag, id);

            //DELETE FROM media
            await DeleteBlob(blobClient, "media", id);
            //DELETE FROM thumbs160
            await DeleteBlob(blobClient, "thumbs160", id);
            //DELETE FROM thumbs320
            await DeleteBlob(blobClient, "thumbs320", id);
            //DELETE FROM thumbs1000
            await DeleteBlob(blobClient, "thumbs1000", id);
            //DELETE FROM vision
            await DeleteBlob(blobClient, "vision", id);
            //DELETE FROM exif
            await DeleteBlob(blobClient, "exif", id);


            return id != null
                ? (ActionResult)new OkObjectResult($"done")
                : new BadRequestObjectResult("Please pass id on the query string ");
        }

        private static async Task<bool> DeleteBlob(CloudBlobClient blobClient, string containerName, string id)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(id);
            return await blob.DeleteIfExistsAsync();
        }
    }
}
