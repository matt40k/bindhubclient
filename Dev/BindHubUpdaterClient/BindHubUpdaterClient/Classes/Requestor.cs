using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog;

namespace BindHub.Client
{
    internal class Requestor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string _apiUrl;
        private string _apiKey;
        private string _apiUser;
        private bool _useProxy;

        public enum RequestType
        {
            Ip,
            All,
            Single,
            Update
        }

        public string SetApiUrl
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _apiUrl = value;
                }
            }
        }

        public string SetApiKey
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _apiKey = value;
                }
            }
        }

        public string SetApiUser
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _apiUser = value;
                }
            }
        }

        public string GetIp
        {
            get
            {
                Uri ipUrl = new Uri(_apiUrl + "ip.json");
                WebRequest request = WebRequest.Create(ipUrl);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                if (false)
                {
                    WebProxy proxy = (WebProxy)WebRequest.DefaultWebProxy;
                    request.Proxy = proxy;
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string result = reader.ReadToEnd();
                if (string.IsNullOrEmpty(result))
                {
                    result = "Empty";
                }

                JObject o = JObject.Parse(result);
                Console.WriteLine((string)o["address"]["public"]);

                return result;
            }
        }

        public string GetAll
        {
            get
            {
                string postData = "user=" + _apiUser + "&key=" + _apiKey;
                string result = null;
                StreamWriter requestWriter;

                Uri ipUrl = new Uri(_apiUrl + "record.json");
                try
                {
                    WebRequest request = WebRequest.Create(ipUrl);
                    request.Method = "POST";
                    //request.ContentType = "application/json";
                    request.ContentType = "application/x-www-form-urlencoded";
                    if (false)
                    {
                        WebProxy proxy = (WebProxy)WebRequest.DefaultWebProxy;
                        request.Proxy = proxy;
                    }
                    //POST the data.
                    using (requestWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        requestWriter.Write(postData);
                    }
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    result = reader.ReadToEnd();
                }
                catch (WebException GetAll_WebException)
                {
                    Console.WriteLine(GetAll_WebException.ToString());
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = "Empty";
                }
                return result;
            }
        }

        public bool SetUseProxy
        {
            set
            {
                _useProxy = value;
            }
        }
    }
}
