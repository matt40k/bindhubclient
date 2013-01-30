/*
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
        static void Main(string[] args)
        {
            bool freeToRun;


            if (args.Length == 0)
            {

                try
                {
                    string safeName = "Global\\BindHubClientMutex";
                    using (Mutex m = new Mutex(true, safeName, out freeToRun))
                        if (freeToRun)
                        {
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
                        else
                        {
                            logger.Log(NLog.LogLevel.Error, "Application is already running!");
                            Environment.Exit(5);
                        }
                }
                catch (Exception)
                {
                    logger.Log(NLog.LogLevel.Error, "Application is already running!");
                    Environment.Exit(5);
                }
            }
            else
            {
                Args _args = new Args(args);

            }
        }
    }
}
