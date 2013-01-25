/*
 * Developer : Matt Smith (matt@matt40k.co.uk)
 * All code (c) Matthew Smith all rights reserved
 */

using System.ServiceProcess;

namespace BindHub.Client.Service
{
    static class Program
    {
        static void Main()
        {
            ServiceBase service = new Service();
            ServiceBase.Run(service);
        }
    }
}