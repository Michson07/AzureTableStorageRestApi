using StorageAccountData.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace StorageAccountData
{
    public class Query
    {
        private readonly RequestSender requestSender;

        //account - Settings -> Access Key -> Storage Account Name
        //key - Settings -> Access Key -> Key1 lub Key2 (bez znaczenia ktory)
        //table - nazwa tabeli, na ktorej beda dokonywane zmiany
        public Query(string account, string sharedKey, string table)
        {
            requestSender = new RequestSender(account, sharedKey, table);
        }

        public string GetAll()
        {
            try
            {
                //w konstruktorze przekazujemy metode zapytania oraz dwie wartosci null, poniewaz nic nie tworzymy
                //ani niczego nie wyszukujemy
                HttpWebRequest request = requestSender.SendRequest("GET", null, null);
                IEnumerable<UpdatedEntity> entity = requestSender.GetResponseQueryForAll(request);

                //zwracanie jsona z otrzymanego obiektu
                return JsonSerializer.Serialize(entity);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetByKeys(StorageEntity searchedEntity)
        {
            try
            {
                HttpWebRequest request = requestSender.SendRequest("GET", null, searchedEntity);
                UpdatedEntity entity = requestSender.GetResponseQuery(request);

                //zwracanie jsona z otrzymanego obiektu
                return JsonSerializer.Serialize(entity);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
