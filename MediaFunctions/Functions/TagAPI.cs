using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using CoreObjects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Functions
{
    public static class TagAPI
    {
        /// <summary>
        /// Replace Tags for an ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tags">comma separated</param>
        /// <returns></returns>
        [FunctionName("saveMediaTags")]
        public static async Task<IActionResult> SaveTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");
            UserProfile U = await UserProfile.Load(req,connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }
            if (!U.isAdmin)
            {
                return new BadRequestObjectResult("No admin rights");
            }

            string id = req.Query["id"];
            string tags = req.Query["tags"];

            CloudTable tableSFI = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);
            CloudTable tableContainerTag = await Azure.GetTableContainerAsync(connectionString, TagList.tableContainerName);
            CloudTable tableContainerTagValues = await Azure.GetTableContainerAsync(connectionString, TagValues.tableContainerName);
            CloudTable tableMediaTag = await Azure.GetTableContainerAsync(connectionString, MediaTag.tableContainerName);
            CloudTable tableTagIndex = await Azure.GetTableContainerAsync(connectionString, TagIndex.tableContainerName);

            var allTags = tags.ToUpper().Split(",");
            foreach (string t in allTags)
            {
                //SAVE THE TAG
                var p = TagIndex.TagSplit(t);
                string tagType = p[0];
                string tagValue = p[1];

                //SAVE TagType
                TagList TL = await TagList.LoadAsync(tableContainerTag, tagType);
                if (TL == null)
                {
                    TL = new TagList() { Tag = tagType };
                    await TL.SaveAsync(tableContainerTag);
                }

                //SAVE VALUES
                TagValues TV = new TagValues() { TagType = tagType, TagValue = tagValue };
                await TV.SaveAsync(tableContainerTagValues);
            }

            //TAG
            await MediaTag.SaveListAsync(tableMediaTag, id, tags.ToUpper());
            //TAGINDEX
            await TagIndex.SaveListAsync(tableTagIndex, id, tags.ToUpper());

            StorageFileInfo SFI = await StorageFileInfo.LoadAsync(tableSFI, id);
            SFI.Tags = tags.ToUpper();
            var r = await SFI.SaveAsync(tableSFI);

            return (ActionResult)new OkObjectResult(r);
        }

        /// <summary>
        /// Add Tags to the ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tags">comma separated</param>
        /// <returns></returns>
        [FunctionName("AddMediaTags")]
        public static async Task<IActionResult> AddMediaTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");
            UserProfile U = await UserProfile.Load(req,connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }
            if (!U.isAdmin)
            {
                return new BadRequestObjectResult("No admin rights");
            }


            string id = req.Query["id"];
            string tags = req.Query["tags"];

            CloudTable tableSFI = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);
            CloudTable tableTagIndex = await Azure.GetTableContainerAsync(connectionString, TagIndex.tableContainerName);
            CloudTable tableMediaTag = await Azure.GetTableContainerAsync(connectionString, MediaTag.tableContainerName);
            CloudTable tableContainerTag = await Azure.GetTableContainerAsync(connectionString, TagList.tableContainerName);
            CloudTable tableContainerTagValues = await Azure.GetTableContainerAsync(connectionString, TagValues.tableContainerName);

            StorageFileInfo SFI= await StorageFileInfo.LoadAsync(tableSFI, id);
            var TagsToAdd = "";

            var allTags = tags.ToUpper().Split(",");
            if (SFI.Tags == null)
                SFI.Tags = "";
            foreach (string t in allTags)
            {
                if (SFI.Tags.IndexOf(t) < 0)
                {
                    if (TagsToAdd != "")
                        TagsToAdd += ",";
                    TagsToAdd += t;

                    //SAVE THE TAG
                    var p = TagIndex.TagSplit(t);
                    string tagType = p[0];
                    string tagValue = p[1];

                    //SAVE TagType
                    TagList TL = await TagList.LoadAsync(tableContainerTag, tagType);
                    if (TL == null)
                    {
                        TL = new TagList() { Tag = tagType };
                        await TL.SaveAsync(tableContainerTag);
                    }

                    //SAVE VALUES
                    TagValues TV = new TagValues() { TagType = tagType, TagValue = tagValue };
                    await TV.SaveAsync(tableContainerTagValues);

                    await TagIndex.SaveAsync(tableTagIndex, id, t);
                    await MediaTag.SaveAsync(tableMediaTag, id, t);
                }

            }
            if (TagsToAdd != "")
            {
                if (SFI.Tags != "")
                    SFI.Tags += ",";
                SFI.Tags += TagsToAdd;
                await SFI.SaveAsync(tableSFI);
            }

            return (ActionResult)new OkObjectResult(true);
        }

        /// <summary>
        /// Return all combination of tag Values
        /// </summary>
        /// <returns>TagValues</returns>
        [FunctionName("getAllTagValues")]
        public static async Task<IActionResult> getAllTagValues(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req,connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }

            CloudTable tableContainer = await Azure.GetTableContainerAsync(connectionString, TagValues.tableContainerName);
            var r = await TagValues.LoadAllAsync(tableContainer);

            return (ActionResult)new OkObjectResult(r);
        }
    }
}
