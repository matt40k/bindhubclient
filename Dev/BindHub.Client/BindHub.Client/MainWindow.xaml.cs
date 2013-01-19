/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLog;

namespace BindHub.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Config _config;
        private DataTable _dt;

        public MainWindow(Config config)
        {
            _config = config;
            InitializeComponent();
        }

        private void IsUsingProxy(bool UseProxy)
        {
            if (UseProxy)
            {
                textProxyAddress.IsEnabled = true;
                textProxyPort.IsEnabled = true;
                checkProxyAuth.IsEnabled = true;
                IsUsingWinAuth(IsWinAuthChecked);
            }
            else
            {
                textProxyAddress.IsEnabled = false;
                textProxyPort.IsEnabled = false;
                checkProxyAuth.IsEnabled = false;
                IsUsingWinAuth(false);
            }
        }

        private void IsUsingWinAuth(bool UseWinAuth)
        {
            if (UseWinAuth)
            {
                textProxyUser.IsEnabled = false;
                textProxyPass.IsEnabled = false;
            }
            else
            {
                textProxyUser.IsEnabled = true;
                textProxyPass.IsEnabled = true;
            }
        }

        private bool IsWinAuthChecked
        {
            get
            {
                bool? value = this.checkProxyAuth.IsChecked;
                if (!value.HasValue)
                {
                    return false;
                }
                if ((bool)value)
                {
                    return true;
                } 
                return false;
            }
        }

        private bool IsProxyChecked
        {
            get
            {
                bool? value = this.checkProxy.IsChecked;
                if (!value.HasValue)
                {
                    return false;
                }
                if ((bool)value)
                {
                    return true;
                } 
                return false;
            }
        }

        private void checkProxy_Checked(object sender, RoutedEventArgs e)
        {
            IsUsingProxy(IsProxyChecked);
        }

        private void checkProxyAuth_Unchecked(object sender, RoutedEventArgs e)
        {
            IsUsingWinAuth(IsWinAuthChecked);
        }

        private void Connected()
        {
            _dt = _config.GetRecords;
            this.dataGrid.DataContext = _dt;
            this.buttonNext.Visibility = Visibility.Hidden;
            this.dataGrid.Visibility = Visibility.Visible;
            this.buttonSave.Visibility = Visibility.Visible;
        }

        private void nextClick(object sender, RoutedEventArgs e)
        {
            bool result = _config.Write(textUser.Text, textPass.Text, textUrl.Text, 5, textProxyAddress.Text, null, textProxyUser.Text, textProxyPass.Text, null);
            if (result)
                Connected();
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            _config.SetRecords = _dt;
            if (_config.Save)
                MessageBox.Show("Config saved");
            this.Close();
        }
    }
}
