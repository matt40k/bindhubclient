﻿/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NLog;

namespace BindHub.Client
{
    class Program : Window
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main()
        {
            bool aIsNewInstance = false;

            Mutex _mutex = new Mutex(true, "BindHub.Client", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                logger.Log(NLog.LogLevel.Error, "Application is already running!");
                Environment.Exit(5);
            }

            Config _config = new Config();
            if (!_config.DoesConfigExist)
            {
                logger.Log(NLog.LogLevel.Info, "Config not found, running config ui");
                Application application = new Application();
                application.Run(new MainWindow(_config));
            }
            if (_config.DoesConfigExist)
            {
                logger.Log(NLog.LogLevel.Info, "Config found");
                Updater _updater = new Updater(_config);
                _updater.Worker();
            }
        }
    }
}
