/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Threading;
using NLog;

namespace BindHub.Client
{
    public class Updater
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Config _config;

        public Updater(Config config)
        {
            _config = config;
        }

        public void Worker()
        {
            _config.ReloadRecords();
            DataTable _dtRecords = _config.GetRecords;
            string _publicIP = _config.GetPublicIp;
            int freq = _config.GetUpdateFrequency;

            while(true)
            {
                logger.Log(NLog.LogLevel.Info, "Checking");
                foreach (DataRow dr in _dtRecords.Rows)
                {
                    if (dr["sync"].ToString().ToLower() == "true")
                    {
                        string target = dr["target"].ToString();
                        string record = dr["record"].ToString();

                        if (dr["target"].ToString() != _publicIP)
                        {
                            logger.Log(NLog.LogLevel.Info, "Updating " + record);
                            _dtRecords = _config.UpdateIp(record, _publicIP);
                        }
                        else
                        {
                            logger.Log(NLog.LogLevel.Info, record + " up-to-date");
                        }
                    }
                }
                Thread.Sleep(freq * 60000);
            }
        }
    }
}
