using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.WindowsAzure.Storage.Table;
using CoreObjects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Functions
{
    public static class SiteFunctions
    {
        [FunctionName("getSAS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, 
            ILogger log)
        {
            string SAS = "not authorized";
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (U.isAuthenticated)
            {
                try
                {
                    SAS = Azure.GetAccountSASToken(connectionString);
                    return (ActionResult)new OkObjectResult(SAS);
                }
                catch (Exception e)
                {
                    return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(e));
                }
            }
            return new BadRequestObjectResult("Not authenticated");
        }

        //string name = req.Query["name"];
        //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //dynamic data = JsonConvert.DeserializeObject(requestBody);

        [FunctionName("getMediaInfo")]
        public static async Task<IActionResult> getMediaInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }

            string id = req.Query["id"];

            StorageFileInfo SFI = await StorageFileInfo.LoadAsync(connectionString, id);
            
            return (ActionResult)new OkObjectResult(SFI);
        }


        /// <summary>
        /// Feeds page detail.htm
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("newsearchdetail")]
        public static async Task<IActionResult> SearchDetail(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SEARCHDETAIL");

            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }


            string SelectedEvent = req.Query["event"].ToString();

            if ((U.allowedTags + ",").IndexOf("DIR:" + SelectedEvent + ",") < 0 && U.allowedTags!="*")
                return new BadRequestObjectResult("Not authenticated to read event:" + SelectedEvent);

            CloudTable tableMedia = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);
            var s = await StorageFileInfo.getFilesFromDirectoryAsync(tableMedia, SelectedEvent);
            return (ActionResult)new OkObjectResult(s);
        }

        /// <summary>
        /// Feeds page main search
        /// </summary>
        /// <returns>List of UE</returns>
        [FunctionName("newsearchmain")]
        public static async Task<IActionResult> Searchv2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SEARCHv2");
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }

            List<UserEvent> LUE = JsonConvert.DeserializeObject<List<UserEvent>>(U.events);

            return (ActionResult)new OkObjectResult(LUE);
        }

    }


}
