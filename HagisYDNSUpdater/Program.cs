using System.ServiceProcess;

namespace HagisYDNSUpdater
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main() 
            => ServiceBase.Run(new ServiceBase[] { new HagisYdnsUpdaterService() });
    }
}
