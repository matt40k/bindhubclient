/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace BindHub.Client.Service
{
    public partial class Service : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Process prc;
        private ManualResetEvent stoppedEvent;
        private readonly string appName = "BindHub.Client.exe";
        private readonly string bindhubConfigName = "bindhub.config";

        public Service()
        {
            InitializeComponent();

            stoppedEvent = new ManualResetEvent(false);
        }

        /// <summary>
        ///     Checks if the service is running
        /// </summary>
        private bool isRunning
        {
            get
            {
                bool freeToRun;
                var safeName = "Global\\BindHubClientMutex";
                using (var m = new Mutex(true, safeName, out freeToRun))
                    m.Close();
                return !freeToRun;
            }
        }

        /// <summary>
        ///     Gets the log file
        /// </summary>
        private string getLogFile
        {
            get
            {
                LoggingConfiguration config = LogManager.Configuration;
                var standardTarget = config.FindTargetByName("System") as FileTarget;
                string expandedFileName = null;

                if (standardTarget != null)
                {
                    expandedFileName = SimpleLayout.Evaluate(standardTarget.FileName.ToString());
                    expandedFileName = expandedFileName.Replace('/', '\\');
                    if (expandedFileName.Substring(0, 1) == "'")
                        expandedFileName = expandedFileName.Substring(1);
                    if (expandedFileName.Substring(expandedFileName.Length - 1) == "'")
                        expandedFileName = expandedFileName.Substring(0, expandedFileName.Length - 1);
                }
                var appDir = Path.GetDirectoryName(expandedFileName);
                logger.Log(LogLevel.Debug, appDir);
                return Path.Combine(appDir, bindhubConfigName);
            }
        }

        protected override void OnStart(string[] args)
        {
            logger.Log(LogLevel.Debug, "Service started");
            ThreadPool.QueueUserWorkItem(ServiceWorkerThread);
        }

        private void ServiceWorkerThread(object state)
        {
            if (File.Exists(getLogFile))
            {
                if (!isRunning)
                {
                    logger.Log(LogLevel.Debug, "Loading Client");
                    prc = new Process();
                    prc.StartInfo.CreateNoWindow = true;
                    prc.StartInfo.UseShellExecute = false;
                    prc.StartInfo.FileName = appName;
                    prc.Start();
                }
            }
            else
            {
                eventLog1.WriteEntry("BindHub Client not generated.");
            }
        }

        /// <summary>
        ///     Kills all the processes when the service stops (gracefully)
        /// </summary>
        protected override void OnStop()
        {
            prc.Kill();
            prc.Close();
            logger.Log(LogLevel.Debug, "Service stopped");
        }
    }
}