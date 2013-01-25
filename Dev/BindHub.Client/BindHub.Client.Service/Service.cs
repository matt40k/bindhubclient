
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace BindHub.Client.Service
{
    public partial class Service : ServiceBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Process prc;
        private string appName = "BindHub.Client.exe";
        private ManualResetEvent stoppedEvent;

        public Service()
        {
            InitializeComponent();

            this.stoppedEvent = new ManualResetEvent(false);
        }

        protected override void OnStart(string[] args)
        {
            logger.Log(NLog.LogLevel.Debug, "Service started");
            ThreadPool.QueueUserWorkItem(new WaitCallback(ServiceWorkerThread));
        }

        private void ServiceWorkerThread(object state)
        {
            if (!isRunning)
            {
                logger.Log(NLog.LogLevel.Debug, "Loading Client");
                prc = new Process();
                prc.StartInfo.CreateNoWindow = true;
                prc.StartInfo.UseShellExecute = false;
                prc.StartInfo.FileName = appName;
                prc.Start();
            }
        }

        private bool isRunning
        {
            get
            {
                bool freeToRun;
                string safeName = "Global\\BindHubClientMutex";
                using (Mutex m = new Mutex(true, safeName, out freeToRun))
                    m.Close();
                return !freeToRun;
            }
        }

        protected override void OnStop()
        {
            prc.Kill();
            prc.Close();
            logger.Log(NLog.LogLevel.Debug, "Service stopped");
        }
    }
}