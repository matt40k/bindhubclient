using System;

namespace BindHub.Client
{
    public class Worker
    {
        private ConfigMan _configMan;
        private string _apiUrl;
        private bool _useProxy;
        private Requestor _requestor;
        private ProxyMan _proxyMan; 

        public void Begin()
        {
            _configMan = new ConfigMan();
            _proxyMan = new ProxyMan();
            _requestor = new Requestor();
            _apiUrl = _configMan.GetApiUrl;

            Uri siteUrl = new Uri(_apiUrl);
            _useProxy = _proxyMan.UseProxy(siteUrl);
            _requestor.SetApiUrl = _apiUrl;
            _requestor.SetUseProxy = _useProxy;
            _requestor.SetApiUser = null;
            _requestor.SetApiKey = null;

            Console.WriteLine(_requestor.GetIp);
            Console.WriteLine();
            Console.WriteLine(_requestor.GetAll);
            Console.ReadKey();
        }
    }
}
