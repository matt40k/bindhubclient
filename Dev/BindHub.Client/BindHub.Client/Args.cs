/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Windows;
using NLog;

namespace BindHub.Client
{
    public class Args
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _reconfig = false;
        private bool _config = false;

        public Args(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string str = args[i].ToLower();
                if (str.StartsWith("--reconfig"))
                {
                    _reconfig = true;
                }

                if (str.StartsWith("--config"))
                {
                    _config = true;
                }
            }

            if (_config || _reconfig)
            {
                Config config = new Config();
                if (_reconfig)
                {
                    if (config.DoesConfigExist)
                    {
                        config.DeleteConfig();
                    }
                }
                Application application = new Application();
                application.Run(new MainWindow(config));

                if (config.IsServiceMode)
                {
                    SvcMan svc = new SvcMan();
                    svc.ReloadService();
                }
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, "Invalid argument set");
                Environment.Exit(10);
            }
        }
    }
}
