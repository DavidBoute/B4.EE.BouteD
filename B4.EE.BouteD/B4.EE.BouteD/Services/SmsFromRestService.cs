using B4.EE.BouteD.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Services
{
    class SmsFromRestService
    {
        public async Task<string> GetJSON(string path)
        {
            var request = WebRequest.Create($"{ConnectionSettings.Prefix}{ConnectionSettings.Host}:{ConnectionSettings.Port}/{ConnectionSettings.Path + path}");
            request.ContentType = "application/json";
            request.Method = "GET";

            string content;

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Debug.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            return content;
                        }
                        else
                        {
                            Debug.WriteLine("Response contained empty body");
                            return string.Empty;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        public async Task<ObservableCollection<SmsDTO>> GetSmsList()
        {
            ObservableCollection<SmsDTO> smsList = new ObservableCollection<SmsDTO>();

            string content = await GetJSON("sms");

            if (content != null)
            {
                var smsJArray = JArray.Parse(content);
                foreach (JObject smsJObject in smsJArray)
                {
                    SmsDTO smsDTO = new SmsDTO
                    {
                        Id = smsJObject["Id"].Value<string>(),
                        Message = smsJObject["Message"].Value<string>(),
                        TimeStamp = smsJObject["TimeStamp"].Value<string>(),
                        StatusId = smsJObject["StatusId"].Value<int>(),
                        StatusName = smsJObject["StatusName"].Value<string>(),
                        ContactId = smsJObject["ContactId"].Value<string>(),
                        ContactFirstName = smsJObject["ContactFirstName"].Value<string>(),
                        ContactLastName = smsJObject["ContactLastName"].Value<string>(),
                        ContactNumber = smsJObject["ContactNumber"].Value<string>()
                    };
                    smsList.Add(smsDTO);
                }
            }

            return smsList;
        }

        public async Task<string> UpdateSms(SmsDTO sms)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{ConnectionSettings.Prefix}{ConnectionSettings.Host}:{ConnectionSettings.Port}/{ConnectionSettings.Path}sms/{sms.Id.ToString()}");
            request.ContentType = "application/json";
            request.Method = "PUT";

            using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                var smsJSON = JsonConvert.SerializeObject(sms);

                streamWriter.Write(smsJSON);
                streamWriter.Flush();
            }

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Debug.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);                        
                    return response.StatusCode.ToString();
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        public async Task<string> DeleteSms(string guid)
        {
            var request = WebRequest.Create($"{ConnectionSettings.Prefix}{ConnectionSettings.Host}:{ConnectionSettings.Port}/{ConnectionSettings.Path}sms/{guid}");
            request.ContentType = "application/json";
            request.Method = "DELETE";

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Debug.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                    return response.StatusCode.ToString();
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        public async Task<ObservableCollection<StatusDTO>> GetStatusList()
        {
            ObservableCollection<StatusDTO> statusList = new ObservableCollection<StatusDTO>();

            string content = await GetJSON("status");

            if (content != null)
            {
                var statusJArray = JArray.Parse(content);
                foreach (JObject statusJObject in statusJArray)
                {
                    StatusDTO statusDTO = new StatusDTO
                    {
                        Id = statusJObject["Id"].Value<int>(),
                        Name = statusJObject["Name"].Value<string>(),
                    };
                    statusList.Add(statusDTO);
                }
            }

            return statusList;
        }

    }
}
