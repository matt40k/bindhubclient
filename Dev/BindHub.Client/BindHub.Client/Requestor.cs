/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.IO;
using System.Net;
using NLog;

namespace BindHub.Client
{
    public class Requestor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string _apiUrl;
        private string _apiKey;
        private string _apiUser;

        private HttpWebResponse request(Uri url, string method, string postData)
        {
            WebRequest request = WebRequest.Create(url);
            StreamWriter requestWriter;
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            if (false)
            {
                WebProxy proxy = (WebProxy)WebRequest.DefaultWebProxy;
                request.Proxy = proxy;
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
                Uri url = new Uri(_apiUrl + "ip.xml");
                HttpWebResponse response = request(url, "GET", null);
                //logger.Log(NLog.LogLevel.Debug, response.StatusCode + " - " + response.StatusDescription);
                ds.ReadXml(response.GetResponseStream());

                if (ds.Tables.Contains("address"))
                    return ds.Tables["address"];

                return null;
            }
        }

        public DataTable GetAll
        {
            get
            {
                DataSet ds = new DataSet("bindhub");
                Uri url = new Uri(_apiUrl + "record.xml");
                string postData = "user=" + _apiUser + "&key=" + _apiKey;
                HttpWebResponse response = request(url, "POST", postData);
                ds.ReadXml(response.GetResponseStream());

                if (ds.Tables.Contains("entity"))
                    return ds.Tables["entity"];

                return null;
            }
        }

        public DataTable UpdateIp(string record, string target)
        {
            DataSet ds = new DataSet("bindhub");
            Uri url = new Uri(_apiUrl + "record/update.xml");
            string postData = "user=" + _apiUser + "&key=" + _apiKey + "&record=" + record + "&target=" + target;
            HttpWebResponse response = request(url, "POST", postData);
            ds.ReadXml(response.GetResponseStream());

            if (ds.Tables.Contains("bindhub"))
                ds.Tables["bindhub"].TableName = "entity";


            foreach (DataTable dt in ds.Tables)
            {
                Console.WriteLine(dt.TableName);
                foreach (DataRow row in dt.Rows) // Loop over the rows.
                {
                    Console.WriteLine("--- Row ---"); // Print separator.
                    foreach (var item in row.ItemArray) // Loop over the items.
                    {
                        Console.Write("Item: "); // Print label.
                        Console.WriteLine(item); // Invokes ToString abstract method.
                    }
                }
            }

            if (ds.Tables.Contains("entity"))
                return ds.Tables["entity"];

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
    }
}
