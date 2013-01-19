/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.IO;
using System.Xml;

namespace BindHub.Client
{
    public class Config
    {
        private string bindhubConfigName = "bindhub.config";
        private DataSet _ds;
        private DataTable _config;
        private DataTable _record;
        private DataTable _address;
        private Requestor _requestor;


        public Config()
        {
            _ds = new DataSet("bindhub");
            _requestor = new Requestor();
        }

        private string bindHubConfigFile
        {
            get
            {
                if (!Directory.Exists(userLocalAppDir))
                    Directory.CreateDirectory(userLocalAppDir);
                return Path.Combine(userLocalAppDir, bindhubConfigName);
            }
        }

        private string userLocalAppDir
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\bindhub";
            }
        }

        public bool DoesConfigExist
        {
            get
            {
                if (File.Exists(bindHubConfigFile))
                {
                    _config = bindHubConfig;
                    _record = bindHubRecord;
                    _requestor.SetApiUrl = GetApiUrl;
                    _requestor.SetApiUser = GetApiUser;
                    _requestor.SetApiKey = GetApiKey;
                    return true;
                }
                return false;
            }
        }

        private DataTable createNewConfig
        {
            get
            {
                _config = new DataTable("config");
                _config.Columns.Add(new DataColumn("api_user", typeof(string)));
                _config.Columns.Add(new DataColumn("api_key", typeof(string)));
                _config.Columns.Add(new DataColumn("api_url", typeof(string)));
                _config.Columns.Add(new DataColumn("update_frequency", typeof(int)));
                _config.Columns.Add(new DataColumn("proxy_address", typeof(string)));
                _config.Columns.Add(new DataColumn("proxy_port", typeof(int)));
                _config.Columns.Add(new DataColumn("proxy_user", typeof(string)));
                _config.Columns.Add(new DataColumn("proxy_password", typeof(string)));
                _config.Columns.Add(new DataColumn("proxy_authentication", typeof(string)));
                return _config;
            }
        }

        private DataTable CreateNewRecordDataTable
        {
            get
            {
                _record = new DataTable("entity");
                _record.Columns.Add(new DataColumn("id", typeof(string)));
                _record.Columns.Add(new DataColumn("record", typeof(string)));
                _record.Columns.Add(new DataColumn("target", typeof(string)));
                _record.Columns.Add(new DataColumn("created", typeof(string)));
                _record.Columns.Add(new DataColumn("last_updated", typeof(string)));
                _record.Columns.Add(new DataColumn("sync", typeof(bool)));
                return _record;
            }
        }

        private DataTable CreateNewAddressDataTable
        {
            get
            {
                _address = new DataTable("address");
                _address.Columns.Add(new DataColumn("public", typeof(string)));
                _address.Columns.Add(new DataColumn("private", typeof(string)));
                return _address;
            }
        }

        public string GetPublicIp
        {
            get
            {
                DataTable address = bindHubAddress;
                DataRow dt = address.Rows[0];
                return dt[1].ToString();
            }
        }

        public void UpdateIp(string record, string target)
        {
            // TODO
            DataTable _ds = _requestor.UpdateIp(record, target);
        }

        public void ReloadRecords()
        {
            // TODO
            if (_ds.Tables.Contains("entity"))
                _ds.Tables.Remove("entity");
            _ds.Tables.Add(bindHubRecord);
        }

        private DataTable bindHubRecord
        {
            get
            {
                _record = CreateNewRecordDataTable;

                if (File.Exists(bindHubConfigFile))
                {
                    XmlTextReader reader = new XmlTextReader(bindHubConfigFile);
                    DataSet ds = new DataSet();
                    ds.ReadXml(reader);
                    if (ds.Tables.Contains("entity"))
                        _record.Merge(ds.Tables["entity"]);
                    reader.Close();
                }
                else
                {
                    DataTable _ds = _requestor.GetAll;
                    _record.Merge(_ds);
                }
                return _record;
            }
        }

         private DataTable bindHubConfig
        {
            get
            {
                _config = createNewConfig;

                if (File.Exists(bindHubConfigFile))
                {
                    XmlTextReader reader = new XmlTextReader(bindHubConfigFile);
                    DataSet ds = new DataSet();
                    ds.ReadXml(reader);
                    if (ds.Tables.Contains("config"))
                        _config.Merge(ds.Tables["config"]);
                    reader.Close();
                }
                return _config;
            }
        }

        private DataTable bindHubAddress
        {
            get
            {
                _address = CreateNewAddressDataTable;
                DataTable _ds = _requestor.GetIp;
                _address.Merge(_ds);
                return _address;
            }
        }

        public bool Write(string user, string api_key, string api_url, int? update_frequency, string proxy_address,
            int? proxy_port, string proxy_user, string proxy_password, int? proxy_authentication)
        {
            DataTable _newConfig = createNewConfig;
            DataRow newrow = _newConfig.NewRow();
            newrow["api_user"] = user;
            newrow["api_key"] = api_key;
            newrow["api_url"] = api_url;
            newrow["update_frequency"] = update_frequency;
            if (!string.IsNullOrEmpty(proxy_address))
            {
                newrow["proxy_address"] = proxy_address;
                newrow["proxy_port"] = proxy_port;
                newrow["proxy_user"] = proxy_user;
                newrow["proxy_password"] = proxy_password;
                newrow["proxy_authentication"] = proxy_authentication;
            }
            _newConfig.Rows.Add(newrow);

            _config = _newConfig;
            _requestor.SetApiUrl = api_url;
            _requestor.SetApiUser = user;
            _requestor.SetApiKey = api_key;


            try
            {
                _record = bindHubRecord;
            }
            catch (Exception Write_Exception)
            {
                System.Windows.MessageBox.Show(Write_Exception.ToString());
                return false;
            }

            return true;
        }

        public bool Save
        {
            get
            {
                if (_ds.Tables.Contains("config"))
                    _ds.Tables.Remove("config");
                if (_ds.Tables.Contains("entity"))
                    _ds.Tables.Remove("entity");
                _ds.Tables.Add(_config);
                _ds.Tables.Add(_record);
                _ds.WriteXml(bindHubConfigFile, XmlWriteMode.WriteSchema);
                return true;
            }
        }

        public DataTable GetRecords
        {
            get
            {
                return _record;
            }
        }

        public DataTable SetRecords
        {
            set
            {
                _record = value;
            }
        }

        public int GetUpdateFrequency
        {
            get
            {
                return (int)_config.Rows[0]["update_frequency"];
            }
        }

        private string GetApiKey
        {
            get
            {
                return (string)_config.Rows[0]["api_key"];
            }
        }

        private string GetApiUser
        {
            get
            {
                return (string)_config.Rows[0]["api_user"];
            }
        }

        private string GetApiUrl
        {
            get
            {
                return (string)_config.Rows[0]["api_url"];
            }
        }
    }
}
