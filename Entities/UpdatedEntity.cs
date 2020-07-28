using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace StorageAccountData.Entities
{
    public class UpdatedEntity : StorageEntity
    {
        public Dictionary<string, string> Properties { get; }

        public UpdatedEntity(string partitionKey, string rowKey, Dictionary<string, string> properties) 
            : base(partitionKey, rowKey)
        {
            this.Properties = properties;
        }
    }
}
