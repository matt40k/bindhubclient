/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using NLog;
using Newtonsoft.Json;

namespace BindHub.Client
{
    public class Requestor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string _apiUrl;
        private string _apiKey;
        private string _apiUser;
        private WebProxy _proxy;
        private bool useProxy;

        private HttpWebResponse request(Uri url, string method, string postData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = GetExe.Product + "\\" + GetExe.Version;
            StreamWriter requestWriter;
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            if (useProxy)
            {
                request.Proxy = _proxy;
            }
            else
            {
                request.Proxy = null;
            }
            if (!string.IsNullOrEmpty(postData))
            {
                using (requestWriter = new StreamWriter(request.GetRequestStream()))
                {
                    requestWriter.Write(postData);
                }
            }
            return (HttpWebResponse)request.GetResponse();
        }

        public DataTable GetIp
        {
            get
            {
                DataSet ds = new DataSet("bindhub");
                string result = null;
                Uri url = new Uri(_apiUrl + "ip.json");
                HttpWebResponse response = request(url, "GET", null);
                logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                //logger.Log(NLog.LogLevel.Debug, result);

                XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(result);
                XmlReader xmlReader = new XmlNodeReader(doc);
                ds.ReadXml(xmlReader);
                if (ds.Tables.Contains("address"))
                {
                    return ds.Tables["address"];
                }
                return null;
            }
        }

        public DataTable GetAll
        {
            get
            {
                DataSet ds = new DataSet("bindhub");
                string result = null;
                Uri url = new Uri(_apiUrl + "record.json");
                string postData = "user=" + _apiUser + "&key=" + _apiKey;
                HttpWebResponse response = request(url, "POST", postData);
                logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                //logger.Log(NLog.LogLevel.Debug, result);

                XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(result);
                XmlReader xmlReader = new XmlNodeReader(doc);
                ds.ReadXml(xmlReader);

                /*
                foreach (DataTable table in ds.Tables)
                    System.Windows.MessageBox.Show(table.TableName);
                 */

                if (ds.Tables.Contains("entities"))
                    return ds.Tables["entities"];

                return null;
            }
        }

        public DataTable UpdateIp(string record, string target)
        {
            DataSet ds = new DataSet("bindhub");
            string result = null;
            Uri url = new Uri(_apiUrl + "record/update.json");
            string postData = "user=" + _apiUser + "&key=" + _apiKey + "&record=" + record + "&target=" + target;
            HttpWebResponse response = request(url, "POST", postData);
            logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            //logger.Log(NLog.LogLevel.Debug, result);

            XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(result);
            XmlReader xmlReader = new XmlNodeReader(doc);
            ds.ReadXml(xmlReader);

            if (ds.Tables.Contains("bindhub"))
                ds.Tables["bindhub"].TableName = "entities";
            if (ds.Tables.Contains("entities"))
                return ds.Tables["entities"];

            return null;
        }

        public string SetApiUrl
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiUrl = value;
            }
        }

        public string SetApiKey
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiKey = value;
            }
        }

        public string SetApiUser
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiUser = value;
            }
        }

        public WebProxy SetWebProxy
        {
            set
            {
                _proxy = value;
            }
        }

        public bool UseProxy
        {
            set
            {
                useProxy = value;
            }
        }
    }
}
