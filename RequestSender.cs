using Newtonsoft.Json.Linq;
using StorageAccountData.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace StorageAccountData
{
    public class RequestSender
    {
        private readonly string account;
        private readonly string table;
        private readonly byte[] sharedKey;
        UpdatedObjectConverter objectConverter = new UpdatedObjectConverter();

        //account - Settings -> Access Key -> Storage Account Name
        //key - Settings -> Access Key -> Key1 lub Key2 (bez znaczenia ktory)
        //table - nazwa tabeli, na ktorej beda dokonywane zmiany
        public RequestSender(string account, string key, string table)
        {
            this.account = account;
            this.table = table;
            sharedKey = Convert.FromBase64String(key);
        }

        private void WriteRequest(HttpWebRequest request, string json)
        {
            //dodawanie do zadania jsona, bedacy obrazem dodawanego/edytowanego rekordu
            using (StreamWriter stream = new StreamWriter(request.GetRequestStream()))
            {
                stream.Write(json);
            }
        }

        private string CreateAuthorizationString(HttpWebRequest request)
        {
            string resource = request.RequestUri.PathAndQuery;
            if (resource.Contains("?"))
            {
                resource = resource.Substring(0, resource.IndexOf("?"));
            }

            //tworzenie signature, który jest kodem HMAC opartym na skrótach (HMAC) 
            //skonstruowanym z żądania i obliczonym przy użyciu algorytmu SHA256, 
            //a następnie zakodowanym przy użyciu kodowania Base64
            string stringToSignature = string.Format($"{request.Headers["x-ms-date"]}\n/{account}{resource}");
            HMACSHA256 hasher = new HMACSHA256(sharedKey);
            string signedSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToSignature)));

            //zwracanie naglowka wedlug wymaganego formatu
            return string.Format($"SharedKeyLite {account}:{signedSignature}");
        }

        public HttpWebRequest SendRequest(string method, string newObject, StorageEntity searchedObject)
        {
            HttpWebRequest request;
            //jesli wyszukiwany jest jakis rekord (dla GET lub PUT) to w linku przekazujemy jego wlasciwosci
            //PartitionKey oraz RowKey
            if (searchedObject == null)
            {
                request = HttpWebRequest.
                    Create($"https://{account}.table.core.windows.net/{table}") as HttpWebRequest;
            }
            else
            {
                request = HttpWebRequest.
                    Create($"https://{account}.table.core.windows.net/{table}(PartitionKey='{searchedObject.PartitionKey}',RowKey='{searchedObject.RowKey}')") as HttpWebRequest;
            }

            request.Method = method;

            //Jesli wykonujemy delete to dodajemy wymagany header If-Match, ktory nawet jak nie znajdzie zasobu to i tak wykona 
            //zadanie
            if (method.Equals("DELETE"))
            {
                request.Headers.Add("If-Match", "*");
            }
            else if (!method.Equals("GET"))
            {
                WriteRequest(request, newObject);
            }

            //dziejsza data w formacie dla np. 28.07.2020 WT, 20 Jul 2020 20:45:30 GMT
            request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));

            //wersja uslugi azure storage
            request.Headers.Add("x-ms-version", "2015-12-11");
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("Authorization", CreateAuthorizationString(request));
            return request;
        }

        public string GetResponseCommand(HttpWebRequest request)
        {
            try
            {
                //pobieranie odpowiedzi z serwera, jesli wszystko jest ok zwracany jest string "Changes saved", 
                //w przeciwnym wypadku wiadomosc dotyczaca bledu
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return "Changes saved";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public IEnumerable<UpdatedEntity> GetResponseQueryForAll(HttpWebRequest request)
        {
            //pobieranie odpowiedzi z serwera
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
            {
                //odpowiedź z serwera
                string responseJson = responseReader.ReadToEnd();
                JToken values = JObject.Parse(responseJson)["value"];
                List<UpdatedEntity> entities = new List<UpdatedEntity>();
                foreach (JToken value in values)
                {
                    //konwersja na slownik
                    Dictionary<string, string> dictionary = value.ToObject<Dictionary<string, string>>();

                    //konwersja na obiekt i dodawanie do listy
                    entities.Add(objectConverter.JsonToEntity(dictionary));
                }

                return entities;
            }
        }

        public UpdatedEntity GetResponseQuery(HttpWebRequest request)
        {
            //pobieranie odpowiedzi z serwera
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
            {
                //odpowiedź z serwera
                string responseJson = responseReader.ReadToEnd();

                //konwertowanie otrzymanego jsona na slownik, a nastepnie zwracanie go jako obiekt UpdatedEntity
                JToken value = JObject.Parse(responseJson);
                Dictionary<string, string> dictionary = value.ToObject<Dictionary<string, string>>();
                dictionary.Remove("odata.metadata");

                return objectConverter.JsonToEntity(dictionary);
            }
        }
    }
}
