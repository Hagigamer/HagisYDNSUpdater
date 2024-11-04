using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HagisYDNSUpdater
{
    public class YdnsSettings
    {
        public bool Logging { get; set; }
        public string ApiUser { get; set; }
        public string ApiKey { get; set; }
        public Dictionary<string, string> Hosts { get; set; } = new Dictionary<string, string>();
        public string Authentication => Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ApiUser}:{ApiKey}"));

        public YdnsSettings(Hashtable hosts)
        {
            foreach (DictionaryEntry entry in hosts)
                Hosts.Add(entry.Key.ToString(), entry.Value.ToString());
        }
    }
}