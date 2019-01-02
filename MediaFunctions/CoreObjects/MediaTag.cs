using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class MediaTag : TableEntity
    {
        public const string tableContainerName = "Tag";

        //ID, Name string, PersonId Guid
        public string Id { get; set; }
        public string Tag { get; set; }
        public string TagType { get; set; }


        public async Task<bool> SaveAsync(CloudTable tableContainer)
        {
            this.PartitionKey = this.Id;
            this.RowKey = this.TagType + ":" + this.Tag.Replace("\\", "|");
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(this));
            return true;
        }
        public static async Task<bool> SaveAsync(CloudTable tableContainer, string Id, string FullTag)
        {
            var a = TagIndex.TagSplit(FullTag);
            MediaTag M = new MediaTag() { Id = Id, Tag = a[1], TagType = a[0] };
            return await M.SaveAsync(tableContainer);
        }
        public static async Task<bool> DeleteByIdAsync(CloudTable tableContainer, string Id)
        {
            List<MediaTag> LMT = await LoadAllAsync(tableContainer, Id);
            foreach (MediaTag MT in LMT)
            {
                TableOperation delete = TableOperation.Delete(MT);
                await tableContainer.ExecuteAsync(delete);
            }
            return true;
        }
        public static async Task<bool> SaveListAsync(CloudTable tableContainer, string Id, string tags)
        {
            var allTags = tags.ToUpper().Split(",");
            //DELETE EXISTING TAGS
            List<MediaTag> LMT = await LoadAllAsync(tableContainer, Id);
            foreach (MediaTag MT in LMT)
            {
                TableOperation delete = TableOperation.Delete(MT);
                await tableContainer.ExecuteAsync(delete);
            }
            foreach (string t in allTags)
            {
                //SAVE THE TAG
                var p = TagIndex.TagSplit(t);
                string tagType = p[0];
                string tagValue = p[1];
                MediaTag MT = new MediaTag() { Id = Id, Tag = tagValue, TagType = tagType };
                await MT.SaveAsync(tableContainer);
            }
            return true;
        }

        /// <summary>
        /// RETURNS ALL TAGS FOR A MEDIA Id
        /// </summary>
        /// <param name="tableContainer"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static async Task<List<MediaTag>> LoadAllAsync(CloudTable tableContainer, string Id)
        {
            return await Azure.GetList<MediaTag>(tableContainer,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Id)
                );
        }

    }

}
