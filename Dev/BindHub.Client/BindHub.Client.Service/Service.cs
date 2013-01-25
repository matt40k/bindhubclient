/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

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
            if (File.Exists(getLogFile))
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
            else
            {
                this.eventLog1.WriteEntry("BindHub Client not generated."); 
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

        private string bindhubConfigName = "bindhub.config";

        private string getLogFile
        {
            get
            {
                LoggingConfiguration config = LogManager.Configuration;
                FileTarget standardTarget = config.FindTargetByName("System") as FileTarget;
                string expandedFileName = null;

                if (standardTarget != null)
                {
                    expandedFileName = NLog.Layouts.SimpleLayout.Evaluate(standardTarget.FileName.ToString());
                    expandedFileName = expandedFileName.Replace('/', '\\');
                    if (expandedFileName.Substring(0, 1) == "'")
                        expandedFileName = expandedFileName.Substring(1);
                    if (expandedFileName.Substring(expandedFileName.Length - 1) == "'")
                        expandedFileName = expandedFileName.Substring(0, expandedFileName.Length - 1);
                }
                string appDir = Path.GetDirectoryName(expandedFileName);
                logger.Log(NLog.LogLevel.Debug, appDir);
                return Path.Combine(appDir, bindhubConfigName);
            }
        }

    }
}