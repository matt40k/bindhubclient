using System.ServiceProcess;

namespace BindHub.Client.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase service = new SampleService();
            ServiceBase.Run(service);
        }
    }
}