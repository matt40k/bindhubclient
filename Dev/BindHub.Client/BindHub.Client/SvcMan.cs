/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System;
using System.ServiceProcess;
using NLog;

namespace BindHub.Client
{
    public class SvcMan
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ServiceController service = new ServiceController("BindHubClientSvc");
        private TimeSpan timeout = TimeSpan.FromMilliseconds(300);

        private void StartService()
        {
            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception StartService_Exception)
            {
                logger.Log(NLog.LogLevel.Error, StartService_Exception);
            }
        }

        private void StopService()
        {
            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception StopService_Exception)
            {
                logger.Log(NLog.LogLevel.Error, StopService_Exception);
            }
        }

        private void RestartService()
        {
            StopService();
            StartService();
        }

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
