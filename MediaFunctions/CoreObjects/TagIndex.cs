using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class TagIndex : BaseEntity
    {
        public static string tableContainerName ="TagIndex"; 

        public override void Keys() { }
        public static async Task<bool> SaveAsync(CloudTable tableContainer, string Id, string Tag)
        {
            TagIndex TI = new TagIndex();
            TI.PartitionKey = Tag.Replace("\\", "|");
            TI.RowKey = Id;
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(TI));
            return true;
        }

        public static async Task<bool> DeleteByIdAsync(CloudTable tableContainer, string Id)
        {
            List<TagIndex> LMT = await LoadByIdAsync(tableContainer, Id);
            foreach (TagIndex MT in LMT)
            {
                TableOperation delete = TableOperation.Delete(MT);
                await tableContainer.ExecuteAsync(delete);
            }
            return true;
        }

        public static async Task<List<TagIndex>> LoadByIdAsync(CloudTable tableContainer, string Id)
        {
            string query = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Id);

            return await Azure.GetList<TagIndex>(tableContainer, query);
        }
        public static async Task<List<TagIndex>> LoadAllAsync(CloudTable tableContainer, string Tag)
        {
            string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Tag);

            return await Azure.GetList<TagIndex>(tableContainer, query);
        }
        public static async Task<bool> SaveListAsync(CloudTable tableContainer, string Id, string tags)
        {
            var allTags = tags.ToUpper().Split(",");
            //DELETE EXISTING TAGS
            List<TagIndex> LMT = await LoadByIdAsync(tableContainer, Id);
            foreach(TagIndex MT in LMT)
            {
                if (MT != null)
                {
                    TableOperation delete = TableOperation.Delete(MT);
                    await tableContainer.ExecuteAsync(delete);
                }
            }
            foreach (string t in allTags)
            {
                await TagIndex.SaveAsync(tableContainer, Id, t);
            }
            return true;
        }
        public static async Task<List<string>> LoadByTagList(CloudTable tableContainer, string[] tagList)
        {
            List<string> res = new List<string>();
            foreach (string tagl in tagList)
            {
                res.AddRange(await LoadByTagList(tableContainer, tagl));
            }
            return res.Distinct().ToList();
        }

        public static int sorte(TagIndex a, TagIndex b)
        {
            return a.RowKey.CompareTo(b.RowKey);
        }
        public static async Task<List<string>> LoadByTagList(CloudTable tableContainer, string tagList)
        {
            List<string> res = new List<string>();

            if (tagList == "")
            {
                string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "");
                List<TagIndex> L = await Azure.GetList<TagIndex>(tableContainer, query);
                L.Sort(sorte);
                string lastRow = "";
                foreach (TagIndex item in L)
                {
                    if (item.RowKey!=lastRow)
                        res.Add(item.RowKey);
                    lastRow = item.RowKey;
                }
            }
            else
            {
                var tagArray = tagList.Split(",");

                var bFirst = true;
                foreach (var tag in tagArray)
                {

                    var t = TagIndex.TagSplit(tag);
                    string newTag = t[0] + ":" + t[1];
                    string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newTag);
                    List<TagIndex> L = await Azure.GetList<TagIndex>(tableContainer, query);

                    if (bFirst)
                    {
                        foreach (TagIndex item in L)
                        {
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
            }
            return res;
        }
        public static async Task<List<TagIndex>> LoadbyTagTypeAsync(CloudTable tableContainer, string tagType)
        {
            //string query = TableQuery.GenerateFilterCondition("TagType", QueryComparisons.Equal, tagType);
            string query =
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, tagType +":"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, tagType + ":xxxxxxxxxxxx")
            );

            return await Azure.GetList<TagIndex>(tableContainer, query);
        }


        public static string[] TagSplit(string tag)
        {
            int p = tag.IndexOf(':');
            if (p >= 0)
            {
                string[] r = { tag.Substring(0, p).Trim(), tag.Substring(p + 1).Trim() };
                return r;
            }
            else
                return new string[] { tag };
        }

    }

}
