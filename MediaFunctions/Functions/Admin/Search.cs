using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.WindowsAzure.Storage.Table;
using CoreObjects;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Functions
{
    /// <summary>
    /// Returns all media for "event" argument. It needs to be in the format "EVENT:name"
    /// </summary>
    public class Search
    {
        //ALL ADMIN 

        [FunctionName("tagSearch")]
        public static async Task<IActionResult> tagsearch(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                ILogger log)
        {
            log.LogInformation("TAGSEARCH");
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

            DateTime d1 = DateTime.Now;
            CloudTable tableMedia = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);

            var s=(from rec in await StorageFileInfo.ListAsync(tableMedia)
                   group rec by rec.Directory into g
                   orderby g.Key
                   select new { @group=g.Key, numMedia=g.Count(), id= g.Min(x => x.Id) } ).ToList();
            log.LogInformation((DateTime.Now-d1).ToString());

            return (ActionResult)new OkObjectResult(s);
        }

        [FunctionName("tagSearchDetail")]
        public static async Task<IActionResult> tagsearchdetail(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("TAGSEARCHDETAIL");
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

            string dir = req.Query["group"].ToString();

            CloudTable tableMedia = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);

            var s = await StorageFileInfo.getFilesFromDirectoryAsync(tableMedia, dir);

            return (ActionResult)new OkObjectResult(s);
        }


        /// <summary>
        /// Return all the ids that follow the tag List
        /// </summary>
        /// <param name="taglist">TagList</param>
        /// <returns>TagValues</returns>
        [FunctionName("UserOptimization")]
        public static async Task<IActionResult> UserOptimization(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SEARCH");
            string connectionString = Environment.GetEnvironmentVariable("Storage");
            CloudTable tableUCP = await Azure.GetTableContainerAsync(connectionString, UserCoreProfile.tableContainerName);

            List<UserCoreProfile> UCP = await UserCoreProfile.List(tableUCP);
            string taglist = req.Query["email"].ToString();

            var d1 = DateTime.Now;
            foreach (var U in UCP)
            {
                if (U.email==taglist || taglist == "*")
                {
                    List<ReturnList> RL = await SearchMediaCore("*", "EVENT", U.email);

                    var r=(from rec in RL
                          group rec by rec.@group into g
                        select new { gr=g.Key, numMedia=g.Count(), id=g.Min(x=>x.id) }).ToList();
                    List<UserEvent> LUE = r.ConvertAll(x => new UserEvent { id = x.id, group = x.gr, numMedia=x.numMedia });

                    var query = (from rec in LUE select rec.@group).Distinct();

                    //WRITE LUE;
                    U.events = JsonConvert.SerializeObject(LUE);
                    U.allowedEventTags = "," + string.Join(",", query.ToArray()) + ",";
                    await U.SaveAsync(tableUCP);
                }
            }


            //EXTRACT EVENT AND STORE IN USERPROFILE
            var d = DateTime.Now - d1;
            return (ActionResult)new OkObjectResult(d);
        }

        /// <summary>
        /// Return all the ids that follow the tag List
        /// </summary>
        /// <param name="taglist">TagList</param>
        /// <returns>TagValues</returns>
        [FunctionName("search")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SEARCH");
            string connectionString = Environment.GetEnvironmentVariable("Storage");

            UserProfile U = await UserProfile.Load(req, connectionString);
            if (!U.isAuthenticated)
            {
                return new BadRequestObjectResult("Not authenticated");
            }

            log.LogInformation(U.email + " " + U.allowedTags);
            string taglist = req.Query["taglist"].ToString().ToUpper();
            string groupBy = req.Query["groupby"].ToString().ToUpper();

            DateTime d1 = DateTime.Now;
            List<ReturnList> RL = await SearchMediaCore(taglist, groupBy, U.email);
            DateTime d53 = DateTime.Now;
            log.LogInformation("Query:" + (d53 - d1) + "=>" + RL.Count);

            return (ActionResult)new OkObjectResult(RL);
        }

        public static async Task<List<ReturnList>> SearchMediaCore(string taglist, string groupBy, string email)
        {

            string connectionString = Environment.GetEnvironmentVariable("Storage");
            CloudTable tableTagIndex = await Azure.GetTableContainerAsync(connectionString, TagIndex.tableContainerName);
            CloudTable tableUCP = await Azure.GetTableContainerAsync(connectionString, UserCoreProfile.tableContainerName);
            CloudTable tableSFI = await Azure.GetTableContainerAsync(connectionString, StorageFileInfo.tableContainerName);

            UserCoreProfile UCP = await UserCoreProfile.Load(tableUCP, email);

            //MEDIA USER CAN ACCESS
            List<string> AuthList = await Search.UserAuthorizedList(UCP.allowedTags, tableTagIndex,tableSFI);
            Console.WriteLine(AuthList.Count);

            List<string> r = await Search.LoadTags(tableTagIndex, taglist, AuthList);
            Console.WriteLine(r.Count);
            Console.WriteLine("TOTAL");

            List<ReturnList> RL = new List<ReturnList>();
            //GROUPBY
            if (string.IsNullOrEmpty(groupBy))
            {
                foreach (string ritem in r)
                {
                    RL.Add(new ReturnList() { id = ritem, group = "" });
                }
            }
            else
            {
                RL = new List<ReturnList>();
                Console.WriteLine("1-TagIndex");
                //var rrr = r.Find(rrrr => rrrr == "0376689d-cd50-4fc6-8c1c-b2a48509f582");
                List<TagIndex> r2 = await TagIndex.LoadbyTagTypeAsync(tableTagIndex, groupBy);

                var RL1 = (from rec in AuthList
                          join p in r2 on rec equals p.RowKey
                          orderby p.PartitionKey
                          select new {id=rec, gr=p.PartitionKey }).ToList();

                //ACRESCENTAR OS QUE NÃO TÊM EVENTO

                RL = RL1.ConvertAll(x => new ReturnList { id = x.id, group = x.gr });

            }

            return RL;
        }

        public static async Task<List<string>> UserAuthorizedList(string tagList, CloudTable tableTagIndex, CloudTable tableMedia)
        {
            if (tagList=="*")
            {
                return (from rec in await StorageFileInfo.ListAsync(tableMedia)
                        select rec.Id).ToList();
            }

            List<string> res = new List<string>();
            var bFirst = true;
            foreach (var tag in tagList.Split(","))
            {
                string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tag);
                List<TagIndex> L = await Azure.GetList<TagIndex>(tableTagIndex, query);

                if (bFirst)
                {
                    foreach (TagIndex item in L)
                    {
                        if (res.Find(r => r == item.RowKey) == null)
                            res.Add(item.RowKey);
                    }
                    bFirst = false;
                }
                else
                {
                    foreach (TagIndex item in L)
                    {
                        if (res.Find(r=>r==item.RowKey)==null)
                            res.Add(item.RowKey);
                    }
                }
            }
            return res;
        }

        public static async Task<List<string>> LoadTags(CloudTable tableTagIndex, string tagList,List<string> AuthList)
        {
            if (tagList == "" || tagList=="*")
            {
                return AuthList;
            }
            else
            {
                List<string> res = new List<string>();
                var tagArray = tagList.Split(",");

                var bFirst = true;
                foreach (var tag in tagArray)
                {

                    var t = TagIndex.TagSplit(tag);
                    string newTag = t[0] + ":" + t[1];
                    List<TagIndex> L = await TagIndex.LoadAllAsync(tableTagIndex, newTag);

                    if (bFirst)
                    {
                        foreach (TagIndex item in L)
                        {
                            if (AuthList.Find(r=>r==item.RowKey)!=null)
                                res.Add(item.RowKey);
                        }
                    }
                    else
                    {
                        for (int i = res.Count - 1; i > -1; i--)
                        {
                            if (L.Find(r => r.RowKey == res[i]) == null)
                            {
                                res.RemoveAt(i);
                            }
                        }
                    }
                    bFirst = false;
                }
                return res;
            }
        }
    }

}
