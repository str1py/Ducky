using Microsoft.Win32;
using System.Collections.Generic;

namespace Ducky.Helpers
{
    class InstalledApps
    {
        Dictionary<string,string> installedapps = new Dictionary<string,string>();
        Dictionary<string, string> programms = new Dictionary<string, string>();
        Dictionary<string, string> games = new Dictionary<string, string>();
        string regKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        public InstalledApps()
        {
            GetInstalledApps();
            DeleteEmptyApps();
        }

        private void GetInstalledApps()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        try
                        {
                            var displayName = subkey.GetValue("DisplayName").ToString();
                            installedapps.Add(displayName, FindByDisplayName(displayName));
                        }
                        catch { }
                    }
                }
            }
        }

        private string FindByDisplayName(string name)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey))
            {
                string[] nameList = key.GetSubKeyNames();
                for (int i = 0; i < nameList.Length; i++)
                {
                    RegistryKey subRegKey = key.OpenSubKey(nameList[i]);
                    try
                    {
                        if (subRegKey.GetValue("DisplayName").ToString() == name)
                        {
                            return subRegKey.GetValue("InstallLocation").ToString();
                        }
                    }
                    catch { }
                }
            }
            return "";
        }

        private void DeleteEmptyApps()
        {
            foreach(KeyValuePair<string, string> pair in installedapps)
            {
                if (pair.Value != "")
                {
                    if(pair.Value.Contains("steam") || pair.Value.Contains("steamapps"))
                        games.Add(pair.Key, pair.Value);
                    else
                        programms.Add(pair.Key, pair.Value);
                }

            }
        }

    }
}
