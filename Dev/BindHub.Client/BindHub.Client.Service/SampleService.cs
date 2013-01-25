using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;


namespace BindHub.Client.Service
{
    public partial class SampleService : ServiceBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Mutex _mutex;
        private Process prc;
        private string appName = "BindHub.Client.exe";
        private bool stopping;
        private ManualResetEvent stoppedEvent;

        public SampleService()
        {
            InitializeComponent();

            this.stopping = false;
            this.stoppedEvent = new ManualResetEvent(false);
        }


        /// <summary>
        /// The function is executed when a Start command is sent to the 
        /// service by the SCM or when the operating system starts (for a 
        /// service that starts automatically). It specifies actions to take 
        /// when the service starts. In this code sample, OnStart logs a 
        /// service-start message to the Application log, and queues the main 
        /// service function for execution in a thread pool worker thread.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <remarks>
        /// A service application is designed to be long running. Therefore, 
        /// it usually polls or monitors something in the system. The 
        /// monitoring is set up in the OnStart method. However, OnStart does 
        /// not actually do the monitoring. The OnStart method must return to 
        /// the operating system after the service's operation has begun. It 
        /// must not loop forever or block. To set up a simple monitoring 
        /// mechanism, one general solution is to create a timer in OnStart. 
        /// The timer would then raise events in your code periodically, at 
        /// which time your service could do its monitoring. The other 
        /// solution is to spawn a new thread to perform the main service 
        /// functions, which is demonstrated in this code sample.
        /// </remarks>
        protected override void OnStart(string[] args)
        {
            // Log a service start message to the Application log.
            //this.eventLog1.WriteEntry("BindHub.Client.Service in OnStart.");

            logger.Log(NLog.LogLevel.Debug, "Service started");

            ThreadPool.QueueUserWorkItem(new WaitCallback(ServiceWorkerThread));
        }

        /// <summary>
        /// The method performs the main function of the service. It runs on 
        /// a thread pool worker thread.
        /// </summary>
        /// <param name="state"></param>
        private void ServiceWorkerThread(object state)
        {
            // Periodically check if the service is stopping.
            while (!this.stopping)
            {
                if (!isRunning)
                {
                    logger.Log(NLog.LogLevel.Debug, "Loading Client");
                    prc = new Process();
                    prc.StartInfo.CreateNoWindow = true;
                    prc.StartInfo.UseShellExecute = false;
                    prc.StartInfo.FileName = appName;
                    prc.Start();
                    prc.WaitForExit();
                }
            }

            // Signal the stopped event.
            this.stoppedEvent.Set();
        }

        private bool isRunning
        {
            get
            {
                bool aIsNewInstance;
                _mutex = new Mutex(true, "BindHub.Client", out aIsNewInstance);
                _mutex.Dispose();
                return !aIsNewInstance;
            }
        }

        /// <summary>
        /// The function is executed when a Stop command is sent to the 
        /// service by SCM. It specifies actions to take when a service stops 
        /// running. In this code sample, OnStop logs a service-stop message 
        /// to the Application log, and waits for the finish of the main 
        /// service function.
        /// </summary>
        protected override void OnStop()
        {
            // Log a service stop message to the Application log.
            //this.eventLog1.WriteEntry("BindHub.Client.Service in OnStop.");
            prc.Close();

            // Indicate that the service is stopping and wait for the finish 
            // of the main service function (ServiceWorkerThread).
            this.stopping = true;
            this.stoppedEvent.WaitOne();

            logger.Log(NLog.LogLevel.Debug, "Service stopped");
        }
    }
}