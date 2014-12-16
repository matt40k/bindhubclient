/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.Data;
using System.Threading;

namespace BindHub.Client.UI
{
    public class Updater
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        public Updater(Config config)
        {
            _config = config;
        }

        /// <summary>
        ///     Worker is the actual logic that runs to see if there
        ///     are any changes and updates if there are
        /// </summary>
        public void Worker()
        {
            _config.ReloadRecords();
            DataTable _dtRecords = _config.GetRecords;
            string _publicIP = _config.GetPublicIp;
            int freq = _config.GetUpdateFrequency;

            while (true)
            {
                logger.Log(LogLevel.Info, "Checking");
                foreach (DataRow dr in _dtRecords.Rows)
                {
                    if (dr["sync"].ToString().ToLower() == "true")
                    {
                        var target = dr["target"].ToString();
                        var record = dr["record"].ToString();

                        if (dr["target"].ToString() != _publicIP)
                        {
                            logger.Log(LogLevel.Info, "Updating " + record);
                            _dtRecords = _config.UpdateIp(record, _publicIP);
                        }
                        else
                        {
                            logger.Log(LogLevel.Info, record + " up-to-date");
                        }
                    }
                }
                Thread.Sleep(freq*60000);
            }
        }
    }
}