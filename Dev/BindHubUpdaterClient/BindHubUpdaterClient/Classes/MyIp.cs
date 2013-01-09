using System;

namespace BindHub.Client
{
    internal class MyIp
    {
        private string _myIp;

        public string GetMyPublicIp
        {
            get
            {
                if (string.IsNullOrEmpty(_myIp))
                {

                }
                return _myIp;
            }
        }

        public void ForceRecheck()
        {

        }
    }
}
