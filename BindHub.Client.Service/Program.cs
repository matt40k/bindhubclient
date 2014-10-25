/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.ServiceProcess;

namespace BindHub.Client.Service
{
    internal static class Program
    {
        /// <summary>
        /// Main entry for Service
        /// </summary>
        private static void Main()
        {
            ServiceBase service = new Service();
            ServiceBase.Run(service);
        }
    }
}