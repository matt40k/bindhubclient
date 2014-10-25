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
using Newtonsoft.Json;
using NLog;

namespace BindHub.Client.Library
{
    public class Requestor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string _apiKey;
        private string _apiUrl;
        private string _apiUser;
        private WebProxy _proxy;
        private bool useProxy;

        /// <summary>
        /// Queries the BindHub service for your current (public) IP
        /// </summary>
        public DataTable GetIp
        {
            get
            {
                var ds = new DataSet("bindhub");
                string result = null;
                var url = new Uri(_apiUrl + "ip.json");
                HttpWebResponse response = request(url, "GET", null);
                logger.Log(LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                //logger.Log(NLog.LogLevel.Debug, result);

                XmlDocument doc = JsonConvert.DeserializeXmlNode(result);
                XmlReader xmlReader = new XmlNodeReader(doc);
                ds.ReadXml(xmlReader);
                if (ds.Tables.Contains("address"))
                {
                    return ds.Tables["address"];
                }
                return null;
            }
        }

        /// <summary>
        /// Queries the BindHub service for your  list of records
        /// </summary>
        public DataTable GetAll
        {
            get
            {
                var ds = new DataSet("bindhub");
                string result = null;
                var url = new Uri(_apiUrl + "record.json");
                string postData = "user=" + _apiUser + "&key=" + _apiKey;
                HttpWebResponse response = request(url, "POST", postData);
                logger.Log(LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                //logger.Log(NLog.LogLevel.Debug, result);
                XmlDocument doc = JsonConvert.DeserializeXmlNode(result, "entities");
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

        /// <summary>
        /// Sets the BindHub API url
        /// </summary>
        public string SetApiUrl
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiUrl = value;
            }
        }

        /// <summary>
        /// Sets the BindHub API key
        /// </summary>
        public string SetApiKey
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiKey = value;
            }
        }

        /// <summary>
        /// Sets the BindHub API Username
        /// </summary>
        public string SetApiUser
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _apiUser = value;
            }
        }

        /// <summary>
        /// Sets the WebProxy
        /// </summary>
        public WebProxy SetWebProxy
        {
            set { _proxy = value; }
        }

        /// <summary>
        /// Sets if the WebProxy is used
        /// </summary>
        public bool UseProxy
        {
            set { useProxy = value; }
        }

        /// <summary>
        /// Returns the HttpWebResponse to the request
        /// </summary>
        /// <param name="url">URL to connect to</param>
        /// <param name="method">POST\GET</param>
        /// <param name="postData">Data to post</param>
        /// <returns></returns>
        private HttpWebResponse request(Uri url, string method, string postData)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
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
            return (HttpWebResponse) request.GetResponse();
        }

        /// <summary>
        /// Connects to BindHub Service and updates the IP address for the record
        /// </summary>
        /// <param name="record"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public DataTable UpdateIp(string record, string target)
        {
            var ds = new DataSet("bindhub");
            string result = null;
            var url = new Uri(_apiUrl + "record/update.json");
            string postData = "user=" + _apiUser + "&key=" + _apiKey + "&record=" + record + "&target=" + target;
            HttpWebResponse response = request(url, "POST", postData);
            logger.Log(LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            //logger.Log(NLog.LogLevel.Debug, result);

            XmlDocument doc = JsonConvert.DeserializeXmlNode(result);
            XmlReader xmlReader = new XmlNodeReader(doc);
            ds.ReadXml(xmlReader);

            if (ds.Tables.Contains("bindhub"))
                ds.Tables["bindhub"].TableName = "entities";
            if (ds.Tables.Contains("entities"))
                return ds.Tables["entities"];

            return null;
        }
    }
}