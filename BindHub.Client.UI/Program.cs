/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Threading;
using System.Windows;
using NLog;

namespace BindHub.Client.UI
{
    internal class Program : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {
            bool freeToRun;


            if (args.Length == 0)
            {
                try
                {
                    string safeName = "Global\\BindHubClientMutex";
                    using (var m = new Mutex(true, safeName, out freeToRun))
                        if (freeToRun)
                        {
                            var _config = new Config();
                            if (!_config.DoesConfigExist)
                            {
                                logger.Log(LogLevel.Info, "Config not found, running config ui");
                                var application = new Application();
                                application.Run(new MainWindow(_config));
                            }
                            if (_config.DoesConfigExist)
                            {
                                logger.Log(LogLevel.Info, "Config found");

                                if (_config.IsServiceMode)
                                {
                                    logger.Log(LogLevel.Info, "Service Mode");
                                }
                                var _updater = new Updater(_config);
                                _updater.Worker();
                            }
                        }
                        else
                        {
                            logger.Log(LogLevel.Error, "Application is already running!");
                            Environment.Exit(5);
                        }
                }
                catch (Exception Main_Exception)
                {
                    logger.Log(LogLevel.Error, Main_Exception);
                    Environment.Exit(5);
                }
            }
            else
            {
                var _args = new Args(args);
            }
        }
    }
}