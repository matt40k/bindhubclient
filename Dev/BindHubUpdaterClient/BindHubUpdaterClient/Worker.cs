using System;

namespace BindHub.Client
{
    public class Worker
    {
        private ConfigMan _configMan;
        private string _apiUrl;
        private string _apiUser;
        private string _apiKey;
        private bool _useProxy;
        private Requestor _requestor;
        private ProxyMan _proxyMan; 

        public void Begin()
        {
            // Load config
            _configMan = new ConfigMan();            
            _apiUrl = _configMan.GetApiUrl;
            _apiUser = _configMan.GetApiUser;
            _apiKey = _configMan.GetApiKey;

            // Load proxy
            _proxyMan = new ProxyMan();
            Uri siteUrl = new Uri(_apiUrl);
            _useProxy = _proxyMan.UseProxy(siteUrl);

            // Load requestor
            _requestor = new Requestor();
            _requestor.SetApiUrl = _apiUrl;
            _requestor.SetApiUser = _apiUser;
            _requestor.SetApiKey = _apiKey;
            _requestor.SetUseProxy = _useProxy;

            Console.WriteLine(_requestor.GetIp);
            Console.WriteLine();
            Console.WriteLine(_requestor.GetAll);
            Console.ReadKey();
        }
    }
}
