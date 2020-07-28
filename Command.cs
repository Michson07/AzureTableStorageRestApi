using StorageAccountData.Entities;
using System.Net;

namespace StorageAccountData
{
    public class Command
    {
        private readonly RequestSender requestSender;

        //account - Settings -> Access Key -> Storage Account Name
        //key - Settings -> Access Key -> Key1 lub Key2 (bez znaczenia ktory)
        //table - nazwa tabeli, na ktorej beda dokonywane zmiany
        public Command(string account, string sharedKey, string table)
        {
            requestSender = new RequestSender(account, sharedKey, table);
        }

        public string Post(UpdatedEntity storageObject)
        {
            //konwersja obiektu na json
            UpdatedObjectConverter jsonConverter = new UpdatedObjectConverter(storageObject);
            string json = jsonConverter.ConvertToJson();

            HttpWebRequest request = requestSender.SendRequest("POST", json, null);

            return requestSender.GetResponseCommand(request);
        }

        public string Put(UpdatedEntity newEntity)
        {
            StorageEntity updatedEntity = new StorageEntity(newEntity.PartitionKey, newEntity.RowKey);

            //konwersja obiektu z nowymi wartosciami na json
            UpdatedObjectConverter jsonConverter = new UpdatedObjectConverter(newEntity);
            string entityJson = jsonConverter.ConvertToJson();

            HttpWebRequest request = requestSender.SendRequest("PUT", entityJson, updatedEntity);

            return requestSender.GetResponseCommand(request);
        }

        public string Delete(StorageEntity storageEntity)
        {
            HttpWebRequest request = requestSender.SendRequest("DELETE", null, storageEntity);
            return requestSender.GetResponseCommand(request);
        }
    }
}
