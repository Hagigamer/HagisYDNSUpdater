using System.ServiceProcess;
using System.Threading;

namespace HagisYDNSUpdater
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
#if DEBUG
            new HagisYdnsUpdaterService().OnDebug();
            Thread.Sleep(Timeout.Infinite);
#else
            ServiceBase.Run(new ServiceBase[] { new HagisYdnsUpdaterService() });
#endif
        }
    }
}
