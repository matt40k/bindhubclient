/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Net;
using NLog;

namespace BindHub.Client.Library
{
    public class Proxy
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _proxy;
        private Uri _uri;

        /// <summary>
        /// Checks is the URL will use a default proxy
        /// </summary>
        private bool useProxy
        {
            get
            {
                WebProxy proxy = WebProxy.GetDefaultProxy();

                // See what proxy is used for resource.
                Uri resourceProxy = proxy.GetProxy(_uri);

                // Test to see whether a proxy was selected.
                if (resourceProxy == _uri)
                {
                    _proxy = null;
                    logger.Log(LogLevel.Debug, "Proxy: None");
                    return false;
                }
                _proxy = resourceProxy.ToString();
                logger.Log(LogLevel.Debug, "Proxy: " + _proxy);
                return true;
            }
        }

        /// <summary>
        /// If the URL does use a default proxy, get the proxy address
        /// </summary>
        public string GetProxyAddress
        {
            get
            {
                if (useProxy)
                {
                    if (string.IsNullOrWhiteSpace(_proxy))
                    {
                        return null;
                    }
                    string[] proxyPart = _proxy.Split(':');
                    if (proxyPart.Length >= 2)
                        return proxyPart[1].Substring(2);
                }
                return null;
            }
        }

        /// <summary>
        /// If the URL does use a default proxy, get the proxy port
        /// </summary>
        public string GetProxyPort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_proxy))
                {
                    return null;
                }
                string[] proxyPart = _proxy.Split(':');

                if (proxyPart.Length >= 3)
                    return proxyPart[2].Substring(0, proxyPart[2].Length - 1);
                return null;
            }
        }

        /// <summary>
        /// Set the URL to be used
        /// </summary>
        public string SetUrl
        {
            set { _uri = new Uri(value); }
        }

        /// <summary>
        /// Builds WebProxy based on the parameters passed
        /// </summary>
        /// <param name="address">Proxy address</param>
        /// <param name="port">Proxy port</param>
        /// <param name="user">Proxy username</param>
        /// <param name="pass">Proxy password</param>
        /// <param name="useWin">Proxy use Windows authentication</param>
        /// <returns></returns>
        public WebProxy GetWebProxy(string address, int? port, string user, string pass, bool? useWin)
        {
            bool _useWin = false;
            int _port = 0;

            if (useWin.HasValue)
                _useWin = (bool) useWin;

            if (port.HasValue)
                _port = (int) port;

            logger.Log(LogLevel.Debug, port + " - " + _port);

            return getWebProxy(address, _port, user, pass, _useWin);
        }

        /// <summary>
        /// Builds WebProxy based on the parameters passed
        /// </summary>
        /// <param name="address">Proxy address</param>
        /// <param name="port">Proxy port</param>
        /// <param name="user">Proxy username</param>
        /// <param name="pass">Proxy password</param>
        /// <param name="useWin">Proxy use Windows authentication</param>
        /// <returns></returns>
        private WebProxy getWebProxy(string address, int port, string user, string pass, bool useWin)
        {
            logger.Log(LogLevel.Debug, "Address: " + address + " - Port: " + port);
            WebProxy _proxy = null;
            try
            {
                _proxy = new WebProxy(address, port);
                _proxy.UseDefaultCredentials = useWin;

                if (!string.IsNullOrWhiteSpace(user))
                {
                    var nc = new NetworkCredential();

                    string[] userParts = user.Split('\\');
                    if (userParts.Length == 2)
                    {
                        nc.UserName = userParts[0];
                        nc.Domain = userParts[1];
                    }
                    else
                    {
                        nc.UserName = user;
                        nc.Domain = null;
                    }
                    nc.Password = pass;

                    _proxy.Credentials = nc;
                }
            }
            catch (Exception getWebProxy_Exception)
            {
                logger.Log(LogLevel.Error, getWebProxy_Exception);
            }
            return _proxy;
        }
    }
}