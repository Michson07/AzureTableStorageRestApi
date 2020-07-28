using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorageAccountData.Entities
{
    public class StorageEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public StorageEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
