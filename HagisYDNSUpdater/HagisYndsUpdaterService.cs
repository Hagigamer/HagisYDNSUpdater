using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Timers;

namespace HagisYDNSUpdater
{
    public partial class HagisYdnsUpdaterService : ServiceBase
    {
        #region Members

        private Timer _timer;
        private YdnsSettings _settings;

        #endregion

        #region Constructor

        public HagisYdnsUpdaterService()
            => InitializeComponent();

        #endregion

        #region Protected Timer Methods

        protected override void OnStart(string[] args)
        {
            WriteToFile($"Service is started at {DateTime.Now}");
            Hashtable settings = ConfigurationManager.GetSection("YDNSSettings") as Hashtable;
            int timerInterval = int.Parse(settings["timerInterval"].ToString()) * 1000;
            _timer = new Timer(timerInterval)
            {
                AutoReset = true,
                Enabled = true
            };
            _settings = ReadSettings();
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            Timer_Elapsed(null, null);
        }

        /// <summary>
        /// Starts in debug session.
        /// </summary>
        public void OnDebug()
            => OnStart(null);

        #endregion

        #region Private Methods

        /// <summary>
        /// Pushes the external ip to all configured hosts.
        /// </summary>
        private void Timer_Elapsed(object sender,
                                   ElapsedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string myIP = webClient.DownloadString("https://ydns.io/api/v1/ip");
            webClient.Headers.Add(HttpRequestHeader.Authorization, $"Basic {_settings.Authentication}");

            if (_settings.Logging)
                WriteToFile($"current IP: {myIP}");
            foreach (KeyValuePair<string, string> host in _settings.Hosts)
                try
                {
                    string result = webClient.DownloadString(string.Format("https://ydns.io/api/v1/update/?host={0}", host.Key));
                    if (_settings.Logging)
                        WriteToFile($"updated host: {host.Key}");
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
                                WriteToFile($"The Web Server returned an error: {webException}");
                                break;
                        }
                    else
                        WriteToFile(exception.ToString());
                }
        }

        /// <summary>
        /// Reads settings file.
        /// </summary>
        private YdnsSettings ReadSettings()
        {
            Hashtable settings = ConfigurationManager.GetSection("YDNSSettings") as Hashtable;
            Hashtable hosts = ConfigurationManager.GetSection("Hosts") as Hashtable;
            YdnsSettings ydnsSettings = new YdnsSettings(hosts)
            {
                Logging = bool.Parse(settings["Logging"].ToString()),
                ApiUser = settings["APIUser"].ToString(),
                ApiKey = settings["APIKey"].ToString()
            };
            return ydnsSettings;
        }

        /// <summary>
        /// Writes the message to a log file in a log folder in the installation directory.
        /// </summary>
        private void WriteToFile(string Message)
        {
            Message = $"{DateTime.Now} — {Message}";
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs");
            using (StreamWriter streamWriter = File.AppendText($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs\\ServiceLog_{DateTime.Now.Date.ToShortDateString()}.log"))
                streamWriter.WriteLine(Message);
        }

        #endregion
    }
}