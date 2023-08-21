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
            string myIP = webClient.DownloadString("https://ydns.io/api/v1/ip");
            string authentication = $"{settings["APIUser"]}:{settings["APIKey"]}";
            authentication = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentication));
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + authentication);
            Hashtable hosts = ConfigurationManager.GetSection("Hosts") as Hashtable;
            if (logging)
                WriteToFile($"current IP: {myIP}");
            foreach (DictionaryEntry host in hosts)
                try
                {
                    string result = webClient.DownloadString(string.Format("https://ydns.io/api/v1/update/?host={0}", host.Key.ToString()));
                    if (logging)
                        WriteToFile($"updated host: {host}");
                }
                catch (Exception exception)
                {
                    if (exception is WebException webException)
                        switch (((HttpWebResponse)webException.Response).StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                WriteToFile("The action could not be performed because the host you'd like to update cannot be found. Please make sure the record exists and is of the correct type: A for IPv4, AAAA for IPv6.");
                                break;
                            case HttpStatusCode.Unauthorized:
                                WriteToFile("The action could not be performed due to authentication issues.");
                                break;
                            case HttpStatusCode.BadRequest:
                                WriteToFile("The action could not be performed due to invalid input parameters.");
                                break;
                            default:
                                WriteToFile("The Web Server returned an error: " + webException);
                                break;
                        }
                    else
                        WriteToFile(exception.ToString());
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
