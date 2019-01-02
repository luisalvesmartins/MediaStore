using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreObjects
{
    public class UserCoreProfile : BaseEntity
    {

        public static string tableContainerName ="usersecurity"; 

        public string email { get; set; }
        public string allowedTags { get; set; }
        public bool isAdmin { get; set; }
        public string events{ get; set; }
        public string allowedEventTags { get; set; }
       
        
        override public void Keys()
        {
            this.PartitionKey = this.email;
            this.RowKey = this.isAdmin.ToString();
        }

        public static async Task<UserCoreProfile> Load(string connectionString, string email)
        {
            CloudTable tableUCP = await Azure.GetTableContainerAsync(connectionString, UserCoreProfile.tableContainerName);
            return await UserCoreProfile.Load(tableUCP, email);
        }
        public static async Task<UserCoreProfile> Load(CloudTable tableContainer, string email)
        {
            return await Azure.Get<UserCoreProfile>(tableContainer,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email)
                );

        }
        public static async Task<List<UserCoreProfile>> List(CloudTable tableContainer)
        {
            return await Azure.GetList<UserCoreProfile>(tableContainer,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan,"")
                );

        }
    }

}
