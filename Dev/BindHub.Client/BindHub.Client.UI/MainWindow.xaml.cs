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

namespace BindHub.Client.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Config _config;
        private DataTable _dt;
        private string _updateFreq;
        private string _proxyAddress;
        private string _proxyPort;
        private string _proxyUser;
        private string _proxyPass;
        private bool _useWin;

        public MainWindow(Config config)
        {
            _config = config;
            InitializeComponent();
            getProxy();
        }

        private void getProxy()
        {
            string proxyAddress = _config.GetProxyAddress;
            string proxyPort = _config.GetProxyPort;

            if (!string.IsNullOrWhiteSpace(proxyAddress))
            {
                this.checkProxy.IsChecked = true;
                IsUsingProxy(true);
                this.textProxyAddress.Text = proxyAddress;
                this.textProxyPort.Text = proxyPort;
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

        private void IsUsingProxy(bool UseProxy)
        {
            if (UseProxy)
            {
                textProxyAddress.Text = _proxyAddress;
                textProxyPort.Text = _proxyPort;
                textProxyUser.Text = _proxyUser;
                textProxyPass.Text = _proxyPass;
                checkProxyAuth.IsChecked = _useWin;

                textProxyAddress.IsEnabled = true;
                textProxyPort.IsEnabled = true;
                checkProxyAuth.IsEnabled = true;
                IsUsingWinAuth(IsWinAuthChecked);
            }
            else
            {
                _proxyAddress = textProxyAddress.Text;
                _proxyPort = textProxyPort.Text;
                _proxyUser = textProxyUser.Text;
                _proxyPass = textProxyPass.Text;
                _useWin = IsWinAuthChecked;

                textProxyAddress.Text = null;
                textProxyPort.Text = null;
                textProxyUser.Text = null;
                textProxyPass.Text = null;
                checkProxyAuth.IsChecked = false;

                textProxyAddress.IsEnabled = false;
                textProxyPort.IsEnabled = false;
                checkProxyAuth.IsEnabled = false;
                IsUsingWinAuth(false);
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

        private bool userPassEntered
        {
            get
            {
                bool userCompleted = !string.IsNullOrWhiteSpace(this.textUser.Text);
                bool passCompleted = !string.IsNullOrWhiteSpace(this.textPass.Text);

                if (userCompleted && passCompleted)
                    return true;

                string incompleteMessage = "Please enter your ";

                if (!userCompleted && !passCompleted)
                    incompleteMessage = incompleteMessage + "username and API key";
                else
                {
                    if (!userCompleted)
                        incompleteMessage = incompleteMessage + "username";
                    if (!passCompleted)
                        incompleteMessage = incompleteMessage + "API key";
                }
                MessageBox.Show(incompleteMessage,"Missing information",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                return false;
            }
        }

        private void nextClick(object sender, RoutedEventArgs e)
        {
            if (userPassEntered)
            {
                bool result = _config.Write(textUser.Text, textPass.Text, textUrl.Text, updateFreq, textProxyAddress.Text, proxyPort, textProxyUser.Text, textProxyPass.Text, _useWin);
                if (result)
                {
                    logger.Log(NLog.LogLevel.Debug, "Config set");
                    Connected();
                }
                else
                {
                    MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private int proxyPort
        {
            get
            {
                try
                {
                    return int.Parse(textProxyPort.Text);
                }
                catch (Exception proxyPort_Exception)
                {
                    logger.Log(NLog.LogLevel.Error, proxyPort_Exception);
                    return 0;
                }
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            _config.SetRecords = _dt;
            if (_config.Save)
            {
                MessageBox.Show("Config saved");
                this.Close();
            }
            else
            {
                MessageBox.Show("Error saving","Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private int updateFreq
        {
            get
            {
                int _updateFreq;
                try
                {
                     _updateFreq = int.Parse(textUpdateFreq.Text);
                }
                catch (Exception updateFreq_Exception)
                {
                    logger.Log(NLog.LogLevel.Error, updateFreq_Exception);
                    return 5;
                }
                logger.Log(NLog.LogLevel.Debug, "updateFreq:" + _updateFreq);
                if (_updateFreq < 5)
                    return 5;
                return _updateFreq;
            }
        }

        private void textUpdateFreq_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _updateFreq = int.Parse(textUpdateFreq.Text).ToString();
            }
            catch (Exception textUpdateFreq_TextChanged_Exception)
            {
                logger.Log(NLog.LogLevel.Error, "Invalid entry for UpdateFreq entered - " + textUpdateFreq.Text);
            }
            textUpdateFreq.Text = _updateFreq;
        }
    }
}
