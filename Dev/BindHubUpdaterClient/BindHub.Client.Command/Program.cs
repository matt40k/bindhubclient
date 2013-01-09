using System;
using BindHub.Client;

namespace BindHub.Client.Command
{
    class Program
    {
        static void Main()
        {
            Worker _worker = new Worker();
            _worker.Begin();
        }
    }
}
