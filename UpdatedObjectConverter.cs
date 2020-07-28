using StorageAccountData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace StorageAccountData
{
    public class UpdatedObjectConverter
    {
        private readonly UpdatedEntity entity;

        public UpdatedObjectConverter(UpdatedEntity entity)
        {
            this.entity = entity;
        }

        public UpdatedObjectConverter()
        {

        }

        private string DictionaryToJson(Dictionary<string, string> dictionary)
        {
            return string.Join(", ", dictionary.Select(i => "\"" + i.Key + "\": \"" + i.Value + "\"").ToArray()) + "}";
        }

        private string StorageEntityToJson()
        {
            return "{" + "\"PartitionKey\": \"" + entity.PartitionKey + "\", \"RowKey\": \"" + entity.RowKey + "\", ";
        }

        public string ConvertToJson()
        {
            string entityJson = StorageEntityToJson();
            entityJson += DictionaryToJson(entity.Properties);

            return entityJson;
        }

        //konwerter jsona do obiektu 
        public UpdatedEntity JsonToEntity(Dictionary<string, string> dictionary)
        {
            string partitionKey = dictionary["PartitionKey"];
            string rowKey = dictionary["RowKey"];

            //usuwanie niepotrzebnych kluczy utworzonych przy konwersji jsona słownik
            dictionary.Remove("PartitionKey");
            dictionary.Remove("RowKey");
            dictionary.Remove("odata.etag");

            return new UpdatedEntity(partitionKey, rowKey, dictionary);
        }
    }
}
