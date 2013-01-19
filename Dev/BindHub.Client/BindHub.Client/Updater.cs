/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Threading;

namespace BindHub.Client
{
    public class Updater
    {
        private Config _config;

        public Updater(Config config)
        {
            _config = config;
        }

        public void Worker()
        {
            DataTable _dtRecords = _config.GetRecords;
            string _publicIP = _config.GetPublicIp;
            int freq = _config.GetUpdateFrequency;

            while(true)
            {
                foreach (DataRow dr in _dtRecords.Rows)
                {
                    if (dr["sync"].ToString().ToLower() == "true")
                    {
                        if (dr["target"].ToString() != _publicIP)
                        {
                            string record = dr["record"].ToString();
                            _config.UpdateIp(record, _publicIP);
                        }
                    }
                }
                Thread.Sleep(freq * 60000);
            }
        }
    }
}
