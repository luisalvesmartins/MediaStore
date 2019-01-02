using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreObjects
{
    abstract public class BaseEntity:TableEntity
    {
        abstract public void Keys();

        public async Task<bool> SaveAsync(CloudTable tableContainer)
        {
            this.Keys();
            await tableContainer.ExecuteAsync(TableOperation.InsertOrReplace(this));
            return true;
        }

    }
}
