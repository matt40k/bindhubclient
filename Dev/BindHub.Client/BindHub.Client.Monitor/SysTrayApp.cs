/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BindHub.Client.Monitor
{
    public class SysTrayApp : Form
    {
        // Reference: http://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/
        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private BackgroundWorker bStatusWorker;
        private int polling = 30;

        public SysTrayApp()
        {
            bool freeToRun;
            string safeName = "Local\\BindHubClientMonitorMutex";
            Mutex m = new Mutex(true, safeName, out freeToRun);
            if (freeToRun)
            {
                trayMenu = new ContextMenu();
                MenuItem title = new MenuItem("BindHub Client", OpenSite);
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
            if (isRunning)
            {
                trayIcon.Icon = new Icon(GetType(), "Icon1.ico");
                status = "running";
                msgBoxIcon = MessageBoxIcon.Information;
            }
            else
            {
                trayIcon.Icon = new Icon(GetType(), "Icon2.ico");
                status = "not running";
                msgBoxIcon = MessageBoxIcon.Error;
            }
            MessageBox.Show("BindHub client is " + status + ". Please refer to the log file for more information", "BindHub", MessageBoxButtons.OK, msgBoxIcon);
        }

        private void OpenLogs(object sender, EventArgs e)
        {
            try
            {
                Process prc = new Process();
                prc.StartInfo.FileName = getLogPath;
                prc.Start();
            }
            catch (System.Exception OpenLogs_LogsException)
            {

            }
        }

        private bool isRunning
        {
            get
            {
                try
                {
                    bool freeToRun;
                    string safeName = "Global\\BindHubClientMutex";
                    using (Mutex m = new Mutex(true, safeName, out freeToRun))
                        m.Close();
                    return !freeToRun;
                }
                catch (Exception)
                {
                    return true;
                }
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
            bStatusWorker.DoWork += new DoWorkEventHandler(ServiceStatusWorker_DoWork);

            if (bStatusWorker.IsBusy != true)
            {
                bStatusWorker.RunWorkerAsync();
            }
        }

        private string getLogPath
        {
            get
            {
                LoggingConfiguration config = LogManager.Configuration;
                FileTarget standardTarget = config.FindTargetByName("System") as FileTarget;

                if (standardTarget != null)
                {
                    string expandedFileName = NLog.Layouts.SimpleLayout.Evaluate(standardTarget.FileName.ToString());
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

        private void ServiceStatusWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool run = true;
            if ((bStatusWorker.CancellationPending == true))
            {
                run = false;
            }
            else
            {
                while (run)
                {
                    Thread.Sleep(polling * 1000);
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
                Process process = new Process();
                process.StartInfo.FileName = "http://bindhubclient.codeplex.com/";
                process.Start();
            }
            catch (Exception OpenSite_Exception)
            {
                
            }
        }
    }
}