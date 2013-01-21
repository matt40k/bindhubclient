using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

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

        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;

        public SysTrayApp()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("BindHub Client");
            trayMenu.MenuItems.Add("Logs", OpenLogs);
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon      = new NotifyIcon();
            trayIcon.Text = "BindHub Client";
            trayIcon.Icon = new Icon(GetType(), "Icon1.ico");

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible     = true;

            if (!IsRunning)
                trayIcon.ShowBalloonTip(1000, "BindHub Client", "BindHub Client is not running", ToolTipIcon.Error);
            else
                trayIcon.ShowBalloonTip(1000, "BindHub Client", "BindHub Client is running", ToolTipIcon.Info);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible       = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenLogs(object sender, EventArgs e)
        {
            try
            {
                Process prc = new Process();
                prc.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bindhub";
                prc.Start();
            }
            catch (System.Exception OpenLogs_LogsException)
            {
                
            }
        }

        private bool IsRunning
        {
            get
            {
                bool aIsNewInstance;
                Mutex _mutex = new Mutex(true, "BindHub.Client", out aIsNewInstance);
                return !aIsNewInstance;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}