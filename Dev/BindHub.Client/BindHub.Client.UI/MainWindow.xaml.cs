/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace BindHub.Client.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Config _config;
        private DataTable _dt;
        private string _proxyAddress;
        private string _proxyPass;
        private string _proxyPort;
        private string _proxyUser;
        private string _updateFreq;
        private bool _useWin;

        public MainWindow(Config config)
        {
            _config = config;
            InitializeComponent();
            getProxy();
        }

        private bool IsWinAuthChecked
        {
            get
            {
                bool? value = checkProxyAuth.IsChecked;
                if (!value.HasValue)
                {
                    return false;
                }
                if ((bool) value)
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
                bool? value = checkProxy.IsChecked;
                if (!value.HasValue)
                {
                    return false;
                }
                if ((bool) value)
                {
                    return true;
                }
                return false;
            }
        }

        private bool userPassEntered
        {
            get
            {
                bool userCompleted = !string.IsNullOrWhiteSpace(textUser.Text);
                bool passCompleted = !string.IsNullOrWhiteSpace(textPass.Text);

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
                MessageBox.Show(incompleteMessage, "Missing information", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
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
                    logger.Log(LogLevel.Error, proxyPort_Exception);
                    return 0;
                }
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
                    logger.Log(LogLevel.Error, updateFreq_Exception);
                    return 5;
                }
                logger.Log(LogLevel.Debug, "updateFreq:" + _updateFreq);
                if (_updateFreq < 5)
                    return 5;
                return _updateFreq;
            }
        }

        private void getProxy()
        {
            string proxyAddress = _config.GetProxyAddress;
            string proxyPort = _config.GetProxyPort;

            if (!string.IsNullOrWhiteSpace(proxyAddress))
            {
                checkProxy.IsChecked = true;
                IsUsingProxy(true);
                textProxyAddress.Text = proxyAddress;
                textProxyPort.Text = proxyPort;
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
            dataGrid.DataContext = _dt;
            buttonNext.Visibility = Visibility.Hidden;
            dataGrid.Visibility = Visibility.Visible;
            buttonSave.Visibility = Visibility.Visible;
        }

        private void nextClick(object sender, RoutedEventArgs e)
        {
            if (userPassEntered)
            {
                bool result = _config.Write(textUser.Text, textPass.Text, textUrl.Text, updateFreq,
                    textProxyAddress.Text, proxyPort, textProxyUser.Text, textProxyPass.Text, _useWin);
                if (result)
                {
                    logger.Log(LogLevel.Debug, "Config set");
                    Connected();
                }
                else
                {
                    MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            _config.SetRecords = _dt;
            if (_config.Save)
            {
                MessageBox.Show("Config saved");
                Close();
            }
            else
            {
                MessageBox.Show("Error saving", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                logger.Log(LogLevel.Error, "Invalid entry for UpdateFreq entered - " + textUpdateFreq.Text);
            }
            textUpdateFreq.Text = _updateFreq;
        }
    }
}