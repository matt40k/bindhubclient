/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace BindHub.Client
{
    class Program : Window
    {
        private Config _config;
        [STAThread]
        static void Main()
        {
            Config _config = new Config();
            if (!_config.DoesConfigExist)
            {
                Application application = new Application();
                application.Run(new MainWindow(_config));
            }
            Updater _updater = new Updater(_config);
            _updater.Worker();
        }
    }
}
