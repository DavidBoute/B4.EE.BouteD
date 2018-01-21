using B4.EE.BouteD.Constants;
using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services.Abstract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace B4.EE.BouteD.Services
{
    class SmsFromRestService : ISmsService
    {

        public async Task<string> GetJSON(string path)
        {
            ConnectionSettings connectionSettings = ConnectionSettings.Instance();

            var request = WebRequest.Create($"{connectionSettings.Prefix}{connectionSettings.Host}:{connectionSettings.Port}/{connectionSettings.Path + path}");
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

        public async void GetSmsList()
        {
            List<SmsDTO> smsList = new List<SmsDTO>();

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

            MessagingCenter.Send<List<SmsDTO>>(smsList, MessagingCenterConstants.SMS_LIST_GET);
        }

        public async void UpdateSms(SmsDTO sms)
        {
            ConnectionSettings connectionSettings = ConnectionSettings.Instance();

            var request = (HttpWebRequest)WebRequest.Create($"{connectionSettings.Prefix}{connectionSettings.Host}:{connectionSettings.Port}/{connectionSettings.Path}sms/{sms.Id}");
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
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MessagingCenter.Send<SmsDTO>(sms, MessagingCenterConstants.SMS_PUT);
                    } 
                    else
                    {
                        Debug.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode.ToString());                      
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error fetching data. Exception: {0}", ex.Message);
            }
        }

        public async void DeleteSms(SmsDTO sms)
        {
            ConnectionSettings connectionSettings = ConnectionSettings.Instance();

            var request = WebRequest.Create($"{connectionSettings.Prefix}{connectionSettings.Host}:{connectionSettings.Port}/{connectionSettings.Path}sms/{sms.Id}");
            request.ContentType = "application/json";
            request.Method = "DELETE";

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MessagingCenter.Send<SmsDTO>(sms, MessagingCenterConstants.SMS_DELETE);
                    }
                    else
                    {
                        Debug.WriteLine("Error deleting data. Server returned status code: {0}", response.StatusCode.ToString());
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Error deleting data. Exception: {0}", ex.Message);
            }
        }

        public async void GetStatusList()
        {
            List<StatusDTO> statusList = new List<StatusDTO>();

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

            MessagingCenter.Send<List<StatusDTO>>(statusList, MessagingCenterConstants.STATUS_LIST_GET);
        }

        // Singleton implementation
        #region Singleton implementation

        private static SmsFromRestService _instance
            = new SmsFromRestService();

        public static SmsFromRestService Instance()
        {
            if (_instance == null)
            {
                _instance = new SmsFromRestService();
            }

            return _instance;
        }

        // private constructor
        private SmsFromRestService() { }

        #endregion
    }
}
