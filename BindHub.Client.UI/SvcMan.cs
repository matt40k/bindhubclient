/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.ServiceProcess;
using NLog;

namespace BindHub.Client.UI
{
    public class SvcMan
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected static string serviceName = "BindHubClientSvc";
        private readonly ServiceController service = new ServiceController(serviceName);
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// 
        /// </summary>
        public bool IsService
        {
            get
            {
                foreach (ServiceController sc in ServiceController.GetServices())
                {
                    if (sc.ServiceName == serviceName)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Starts the BindHub.Client as a Windows Service
        /// </summary>
        private void StartService()
        {
            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception StartService_Exception)
            {
                logger.Log(LogLevel.Error, StartService_Exception);
            }
        }

        /// <summary>
        /// Stops the BindHub.Client running (as a Windows Service)
        /// </summary>
        private void StopService()
        {
            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception StopService_Exception)
            {
                logger.Log(LogLevel.Error, StopService_Exception);
            }
        }

        /// <summary>
        /// Stops and starts the BindHub.Client service
        /// </summary>
        private void RestartService()
        {
            StopService();
            StartService();
        }

        /// <summary>
        /// Reloads the BindHub.Client service status 
        /// and restarts the service or starts it if its stopped
        /// </summary>
        public void ReloadService()
        {
            ServiceControllerStatus status = service.Status;

            switch (status)
            {
                case ServiceControllerStatus.Stopped:
                    StartService();
                    break;
                default:
                    ReloadService();
                    break;
            }
        }
    }
}