/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Net;
using NLog;

namespace BindHub.Client
{
    public class Config
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string bindhubConfigName = "bindhub.config";
        private DataSet _ds;
        private DataTable _config;
        private DataTable _record;
        private DataTable _address;
        private Requestor _requestor;
        private Proxy _proxy;


        public Config()
        {
            _ds = new DataSet("bindhub");
            _requestor = new Requestor();
            _proxy = new Proxy();
            _proxy.SetUrl = "https://www.bindhub.com/api/";
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

                    bool useProxy = false;
                    _requestor.UseProxy = useProxy;
                    if (useProxy)
                        _requestor.SetWebProxy = _proxy.GetWebProxy(GetConfigProxyAddress, GetConfigProxyPort, GetConfigProxyUser, GetConfigProxyPassword, GetConfigProxyWinAuth);

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
                _config.Columns.Add(new DataColumn("proxy_winauth", typeof(bool)));
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
                
                DataColumn syncColumn = new DataColumn("sync", typeof(bool));
                syncColumn.DefaultValue = false;
                _record.Columns.Add(syncColumn);

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

        public DataTable UpdateIp(string record, string target)
        {
            DataTable _dt = _requestor.UpdateIp(record, target);
            if (_dt != null)
            {
                foreach (DataRow row in _record.Rows)
                {
                    if (row["record"].ToString() == record)
                        row.SetField("target", target);
                }
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, "Update failed");
            }
            bool save = Save;
            logger.Log(NLog.LogLevel.Debug, "Save status" + save);
            return _record;
        }

        public void ReloadRecords()
        {
            DataTable configDataTable = _record;
            DataTable reloadDataTable = CreateNewRecordDataTable;

            DataTable _getDt = _requestor.GetAll;
            reloadDataTable.Merge(_getDt);

            foreach (DataRow dr in reloadDataTable.Rows)
            {
                string reloadId = (string)dr["id"];
                string reloadRecord = (string)dr["record"];
                string reloadTarget = (string)dr["target"];
                string reloadCreated = (string)dr["created"];
                string reloadLastUpdated = (string)dr["last_updated"];

                bool rowFound = false;

                foreach (DataRow row in configDataTable.Rows)
                {
                    if (row["record"].ToString() == reloadRecord)
                    {
                        if (row["target"].ToString() != reloadTarget)
                        {
                            logger.Log(NLog.LogLevel.Info, reloadRecord + " appears to have changed since last sync");
                            row.SetField("id", reloadId);
                            row.SetField("target", reloadTarget);
                            row.SetField("created", reloadCreated);
                            row.SetField("last_updated", reloadLastUpdated);
                        }
                        rowFound = true;
                    }
                }

                if (!rowFound)
                {
                    logger.Log(NLog.LogLevel.Info, "Found new record - " + reloadRecord);
                    DataRow newrow = configDataTable.NewRow();
                    newrow["id"] = reloadId;
                    newrow["record"] = reloadRecord;
                    newrow["target"] = reloadTarget;
                    newrow["created"] = reloadCreated;
                    newrow["last_updated"] = reloadLastUpdated;
                    newrow["sync"] = false;
                    configDataTable.Rows.Add(newrow);
                }
            }

            logger.Log(NLog.LogLevel.Debug, "Reload");

            _record = configDataTable;
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

        public bool Write(string user, string api_key, string api_url, int update_frequency, string proxy_address,
            int? proxy_port, string proxy_user, string proxy_password, bool proxy_authentication)
        {
            logger.Log(NLog.LogLevel.Debug, proxy_port);
            bool useProxy = (!string.IsNullOrEmpty(proxy_address));

            DataTable _newConfig = createNewConfig;
            DataRow newrow = _newConfig.NewRow();
            newrow["api_user"] = user;
            newrow["api_key"] = api_key;
            newrow["api_url"] = api_url;
            newrow["update_frequency"] = update_frequency;
            if (useProxy)
            {
                newrow["proxy_address"] = proxy_address;
                newrow["proxy_port"] = proxy_port;
                if (!string.IsNullOrEmpty(proxy_user))
                {
                    newrow["proxy_user"] = proxy_user;
                    newrow["proxy_password"] = proxy_password;
                }
                if ((bool)proxy_authentication)
                    newrow["proxy_winauth"] = proxy_authentication;
            }
            _newConfig.Rows.Add(newrow);

            _config = _newConfig;

            _requestor.UseProxy = useProxy;
            if (useProxy)
                _requestor.SetWebProxy = _proxy.GetWebProxy(proxy_address, proxy_port, proxy_user, proxy_password, proxy_authentication);
            _requestor.SetApiUrl = api_url;
            _requestor.SetApiUser = user;
            _requestor.SetApiKey = api_key;

            try
            {
                _record = bindHubRecord;
            }
            catch (Exception Write_Exception)
            {
                logger.Log(NLog.LogLevel.Error, Write_Exception.ToString);
                return false;
            }

            return true;
        }

        public bool Save
        {
            get
            {
                try
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
                catch (Exception Save_Exception)
                {
                    logger.Log(NLog.LogLevel.Error, Save_Exception);
                }
                return false;
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

        public string GetProxyAddress
        {
            get
            {
                return _proxy.GetProxyAddress;
            }
        }

        public string GetProxyPort
        {
            get
            {
                return _proxy.GetProxyPort;
            }
        }

        public int GetIntProxyPort
        {
            get
            {
                try
                {
                    return int.Parse(_proxy.GetProxyPort);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        private string GetConfigProxyAddress
        {
            get
            {
                return (string)_config.Rows[0]["proxy_address"];
            }
        }

        private int? GetConfigProxyPort
        {
            get
            {
                return (int?)_config.Rows[0]["proxy_port"];
            }
        }

        private string GetConfigProxyUser
        {
            get
            {
                return (string)_config.Rows[0]["proxy_user"];
            }
        }

        private string GetConfigProxyPassword
        {
            get
            {
                return (string)_config.Rows[0]["proxy_password"];
            }
        }

        private bool? GetConfigProxyWinAuth
        {
            get
            {
                return (bool?)_config.Rows[0]["proxy_winauth"];
            }
        }
    }
}
