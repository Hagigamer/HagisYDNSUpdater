using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace HagisYDNSUpdater
{
    public partial class HagisYdnsUpdaterService : ServiceBase
    {
        Timer timer;
        public HagisYdnsUpdaterService()
            => InitializeComponent();

        protected override void OnStart(string[] args)
        {
            WriteToFile($"Service is started at {DateTime.Now}");
            Timer_Elapsed(null, null);
            Hashtable settings = ConfigurationManager.GetSection("YDNSSettings") as Hashtable;
            int timerInterval = int.Parse(settings["timerInterval"].ToString()) * 1000;
            timer = new Timer(timerInterval)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
        }

        /// <summary>
        /// Pushes the external ip to all configured hosts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Hashtable settings = ConfigurationManager.GetSection("YDNSSettings") as Hashtable;
            bool logging = bool.Parse(settings["Logging"].ToString());
            WebClient webClient = new WebClient();
            string myIP = webClient.DownloadString("http://myexternalip.com/raw");
            string authentication = $"{settings["APIUser"]}:{settings["APIKey"]}";
            authentication = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentication));
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + authentication);
            Hashtable hosts = ConfigurationManager.GetSection("Hosts") as Hashtable;
            if (logging)
                WriteToFile("current IP: " + myIP);
            foreach (DictionaryEntry host in hosts)
            {
                webClient.DownloadString(string.Format("https://ydns.io/api/v1/update/?host={0}&ip={1}", host.Value.ToString(), myIP));
                if (logging)
                    WriteToFile($"updated host: {host}");
            }
        }

        /// <summary>
        /// Writes the message to a log file in a log folder in the installation directory.
        /// </summary>
        /// <param name="Message"></param>
        public void WriteToFile(string Message)
        {
            Message = $"{DateTime.Now} — {Message}";
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs");
            using (StreamWriter streamWriter = File.AppendText($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs\\ServiceLog_{DateTime.Now.Date.ToShortDateString()}.log"))
                streamWriter.WriteLine(Message);
        }
    }
}
