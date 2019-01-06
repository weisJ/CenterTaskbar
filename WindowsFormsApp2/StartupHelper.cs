using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CenterTaskbar
{
    class StartupHelper
    {
        private readonly RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private string appName;
        private int framerate;

        public StartupHelper(string appName, int framerate)
        {
            this.appName = appName;
            this.framerate = framerate;
        }


        /// <summary>
        /// Toggles whether the application runs on startup.
        /// </summary>
        /// <param name="sender">Sender of the Event</param>
        /// <param name="e">Event arguments</param>
        public void ToggleStartup(object sender, EventArgs e)
        {
            if (IsApplicationInStatup())
            {
                RemoveApplicationFromStartup();
                (sender as MenuItem).Checked = false;
            }
            else
            {
                AddApplicationToStartup();
                (sender as MenuItem).Checked = true;
            }
        }

        /// <summary>
        /// Returns whether the application is run on system startup
        /// </summary>
        /// <returns>true if applications runs on startup</returns>
        public bool IsApplicationInStatup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key == null) return false;

                object value = key.GetValue(appName);
                if (value is String) return ((value as String).StartsWith("\"" + Application.ExecutablePath + "\""));

                return false;
            }
        }

        public void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(appName, "\"" + Application.ExecutablePath + "\" " + framerate);
            }
        }

        public void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(appName, false);
            }
        }
    }
}
