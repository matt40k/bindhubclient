/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace BindHub.Client.Monitor
{
    public class SysTrayApp : Form
    {
        // Reference: http://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/

        private readonly NotifyIcon trayIcon;
        private BackgroundWorker bStatusWorker;
        private int polling = 30;
        private ContextMenu trayMenu;

        public SysTrayApp()
        {
            bool freeToRun;
            string safeName = "Local\\BindHubClientMonitorMutex";
            var m = new Mutex(true, safeName, out freeToRun);
            if (freeToRun)
            {
                trayMenu = new ContextMenu();
                var title = new MenuItem("BindHub Client", OpenSite);
                title.DefaultItem = true;
                trayMenu.MenuItems.Add(title);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Status", Status);
                trayMenu.MenuItems.Add("Logs", OpenLogs);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Exit", OnExit);

                trayIcon = new NotifyIcon();
                trayIcon.Text = "BindHub Client";
                trayIcon.Icon = new Icon(GetType(), "Icon1.ico");
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;

                if (isRunning)
                {
                    trayIcon.Icon = new Icon(GetType(), "Icon1.ico");
                    trayIcon.ShowBalloonTip(1000, "BindHub Client", "BindHub Client is running", ToolTipIcon.Info);
                }
                else
                {
                    trayIcon.Icon = new Icon(GetType(), "Icon2.ico");
                    trayIcon.ShowBalloonTip(1000, "BindHub Client", "BindHub Client is not running", ToolTipIcon.Error);
                }
                Poll();
            }
            else
            {
                Environment.Exit(5);
            }
        }

        /// <summary>
        /// Checks is the application is already running - we limit to only once instance per-user.
        /// </summary>
        private bool isRunning
        {
            get
            {
                try
                {
                    bool freeToRun;
                    string safeName = "Global\\BindHubClientMutex";
                    using (var m = new Mutex(true, safeName, out freeToRun))
                        m.Close();
                    return !freeToRun;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Get the Log path
        /// </summary>
        private string getLogPath
        {
            get
            {
                LoggingConfiguration config = LogManager.Configuration;
                var standardTarget = config.FindTargetByName("System") as FileTarget;

                if (standardTarget != null)
                {
                    string expandedFileName = SimpleLayout.Evaluate(standardTarget.FileName.ToString());
                    expandedFileName = expandedFileName.Replace('/', '\\');
                    if (expandedFileName.Substring(0, 1) == "'")
                        expandedFileName = expandedFileName.Substring(1);
                    if (expandedFileName.Substring(expandedFileName.Length - 1) == "'")
                        expandedFileName = expandedFileName.Substring(0, expandedFileName.Length - 1);
                    return expandedFileName;
                }
                return null;
            }
        }

        /// <summary>
        /// Checks if the application is running as a service or in user-mode
        /// </summary>
        private bool IsServiceMode
        {
            get
            {
                LoggingConfiguration config = LogManager.Configuration;
                var standardTarget = config.FindTargetByName("System") as FileTarget;
                bool multiConfig =
                    standardTarget.FileName.ToString().Contains("${specialfolder:folder=CommonApplicationData}");
                if (multiConfig)
                {
                    return IsService;
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if the application is a service
        /// </summary>
        private bool IsService
        {
            get
            {
                foreach (ServiceController sc in ServiceController.GetServices())
                {
                    if (sc.ServiceName == "BindHubClientSvc")
                        return true;
                }
                return false;
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Status(object sender, EventArgs e)
        {
            string status;
            MessageBoxIcon msgBoxIcon;
            MessageBoxButtons msgBoxButtons;
            if (isRunning)
            {
                trayIcon.Icon = new Icon(GetType(), "Icon1.ico");
                status = "running.";
                msgBoxIcon = MessageBoxIcon.Information;
                msgBoxButtons = MessageBoxButtons.OK;
            }
            else
            {
                trayIcon.Icon = new Icon(GetType(), "Icon2.ico");
                status = "not running.\nWould you like to start the client?";
                msgBoxIcon = MessageBoxIcon.Error;
                msgBoxButtons = MessageBoxButtons.YesNo;
            }
            DialogResult dialogResult =
                MessageBox.Show(
                    "BindHub client is " + status + "\n\nPlease refer to the log file for more information", "BindHub",
                    msgBoxButtons, msgBoxIcon);
            if (dialogResult == DialogResult.Yes)
            {
                if (!IsServiceMode)
                {
                    var prc = new Process();
                    prc.StartInfo.FileName = "services.msc";
                    prc.Start();
                }
                else
                {
                    var prc = new Process();
                    prc.StartInfo.FileName = "BindHub.Client.exe";
                    prc.Start();
                }
            }
        }

        private void OpenLogs(object sender, EventArgs e)
        {
            try
            {
                var prc = new Process();
                prc.StartInfo.FileName = getLogPath;
                prc.Start();
            }
            catch (Exception OpenLogs_LogsException)
            {
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

        private void Poll()
        {
            bStatusWorker = new BackgroundWorker();
            bStatusWorker.WorkerReportsProgress = false;
            bStatusWorker.WorkerSupportsCancellation = false;
            bStatusWorker.DoWork += ServiceStatusWorker_DoWork;

            if (bStatusWorker.IsBusy != true)
            {
                bStatusWorker.RunWorkerAsync();
            }
        }

        private void ServiceStatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool run = true;
            if (bStatusWorker.CancellationPending)
            {
                run = false;
            }
            else
            {
                while (run)
                {
                    Thread.Sleep(polling*1000);
                    if (isRunning)
                        trayIcon.Icon = new Icon(GetType(), "Icon1.ico");
                    else
                        trayIcon.Icon = new Icon(GetType(), "Icon2.ico");
                }
            }
        }

        private void OpenSite(object sender, EventArgs e)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "http://bindhubclient.codeplex.com/";
                process.Start();
            }
            catch (Exception OpenSite_Exception)
            {
            }
        }
    }
}