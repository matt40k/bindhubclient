using System;
using System.Configuration;
using NLog;

namespace BindHub.Client
{
    internal class ConfigMan
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string _ApiKey;
        private string _ApiUrl;
        private string _ApiUser;

        public string GetApiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_ApiKey))
                {
                    _ApiKey = ConfigurationManager.AppSettings["API_Key"];
                }
                return _ApiKey;
            }
        }

        public string GetApiUser
        {
            get
            {
                if (string.IsNullOrEmpty(_ApiUser))
                {
                    _ApiUser = ConfigurationManager.AppSettings["API_User"];
                }
                return _ApiUser;
            }
        }

        public string SetApiKey
        {
            set
            {
                _ApiKey = value;
                logger.Log(NLog.LogLevel.Debug, "SetApiKey: " + _ApiKey);

                // Reference: Bin-ze Zhao - http://social.msdn.microsoft.com/Forums/da-DK/csharpgeneral/thread/77b87843-ae0b-463d-b50e-b6b8e9175e50
                // Open App.Config of executable
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                // Add an Application Setting.
                config.AppSettings.Settings.Remove("API_Key");
                config.AppSettings.Settings.Add("API_Key", _ApiKey);
                try
                {
                    // Save the configuration file.
                    config.Save(ConfigurationSaveMode.Modified);
                }
                catch (Exception writeConfig_UpdatesException)
                {
                    logger.Log(NLog.LogLevel.Error, writeConfig_UpdatesException);
                }
                // Force a reload of a changed section.
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public string GetApiUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_ApiUrl))
                {
                    _ApiUrl = ConfigurationManager.AppSettings["API_Url"];
                }
                if (string.IsNullOrEmpty(_ApiUrl))
                {
                    _ApiUrl = "https://www.bindhub.com/api/";
                }
                return _ApiUrl;
            }
        }
    }
}
