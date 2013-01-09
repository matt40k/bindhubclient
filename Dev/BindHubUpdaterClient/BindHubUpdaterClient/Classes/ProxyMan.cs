using System;
using System.Net;
using NLog;

namespace BindHub.Client
{
    internal class ProxyMan
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool UseProxy(Uri resource)
        {
            WebProxy proxy = (WebProxy)WebProxy.GetDefaultProxy();

            // See what proxy is used for resource.
            Uri resourceProxy = proxy.GetProxy(resource);

            // Test to see whether a proxy was selected.
            if (resourceProxy == resource)
            {
                logger.Log(NLog.LogLevel.Debug, "Proxy: None");
                return false;
            }
            else
            {
                logger.Log(NLog.LogLevel.Debug, "Proxy: " + resourceProxy.ToString());
                return true;
            }
        }
    }
}
