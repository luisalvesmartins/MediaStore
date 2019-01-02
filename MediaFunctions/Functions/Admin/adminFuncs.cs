using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using CoreObjects;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Functions
{
    public static class adminFuncs
    {
        [FunctionName("admListUsers")]
        public static async Task<IActionResult> admListUsers(
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

            CloudTable tableUCP = await Azure.GetTableContainerAsync(connectionString, UserCoreProfile.tableContainerName);
            var a = await UserCoreProfile.List(tableUCP);

            return (ActionResult)new OkObjectResult(a);
        }
        [FunctionName("admSaveUserPermissions")]
        public static async Task<IActionResult> admSaveUserPermissions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
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

            CloudTable tableUCP = await Azure.GetTableContainerAsync(connectionString, UserCoreProfile.tableContainerName);
            CloudTable tableMedia = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);

            string email = req.Query["email"];
            string allowedTags = await new StreamReader(req.Body).ReadToEndAsync();

            UserCoreProfile u = await UserCoreProfile.Load(tableUCP,email);
            u.allowedTags = allowedTags;

            var s = (from rec in await StorageFileInfo.ListAsync(tableMedia)
                     group rec by rec.Directory into g
                     orderby g.Key
                     select new { @group = g.Key, numMedia = g.Count(), id = g.Min(x => x.Id) }).ToList();
            List<UserEvent> LUE = new List<UserEvent>();

            //REMOVE THE ONES NOT IN ALLOWEDTAGS
            var a = allowedTags.Split(",");
            foreach (var item in s.ConvertAll(x => new UserEvent { id = x.id, group = x.group, numMedia = x.numMedia }))
            {
                if (("," + allowedTags + ",").IndexOf(":" + item.group + ",") >= 0)
                    LUE.Add(item);
            }

            //WRITE LUE;
            u.events = JsonConvert.SerializeObject(LUE);

            await u.SaveAsync(tableUCP);

            return (ActionResult)new OkObjectResult(true);
        }

    }
}
