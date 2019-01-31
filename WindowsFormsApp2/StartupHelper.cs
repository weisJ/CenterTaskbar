namespace CenterTaskbar
{
    using Microsoft.Win32;
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="StartupHelper" />
    /// </summary>
    internal static class StartupHelper
    {
        /// <summary>
        /// Name of the application.
        /// </summary>
        private static string appName;

        /// <summary>
        /// Path of application.
        /// </summary>
        private static string appPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupHelper"/> class.
        /// </summary>
        /// <param name="appName">The appName<see cref="string"/></param>
        /// <param name="framerate">The framerate<see cref="int"/></param>
        public static void SetupStartupHelper(string appName, int framerate)
        {
            StartupHelper.appName = appName + "(" + framerate + ")";
            StartupHelper.appPath = Application.ExecutablePath;
        }

        /// <summary>
        /// Toggles whether the application runs on startup.
        /// </summary>
        /// <param name="sender">Sender of the Event</param>
        /// <param name="e">Event arguments</param>
        public static void ToggleStartup(object sender, EventArgs e)
        {
            if (IsApplicationInStatup())
            {
                if (RemoveApplicationFromStartup())
                {
                    (sender as MenuItem).Checked = false;
                }
            }
            else
            {
                if (AddApplicationToStartup())
                {
                    (sender as MenuItem).Checked = true;
                }
            }
        }

        /// <summary>
        /// Returns whether the application is run on system startup
        /// </summary>
        /// <returns>true if applications runs on startup</returns>
        public static bool IsApplicationInStatup()
        {
            RegistryKey rk;
            string value;

            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                value = rk.GetValue(appName).ToString();
                return !(value == null || !value.ToLower().Equals(appPath.ToLower()));
            }
            catch (Exception)
            {
            }

            try
            {
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                value = rk.GetValue(appName).ToString();
                return !(value == null || !value.ToLower().Equals(appPath.ToLower()));
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Add application to startup
        /// </summary>
        /// <returns>true if application has been successfully removed</returns>
        public static bool AddApplicationToStartup()
        {
            RegistryKey rk;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue(appName, appPath);
                return true;
            }
            catch (Exception)
            {
            }

            try
            {
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue(appName, appPath);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Remove the application from startup
        /// </summary>
        /// <returns>true if application has been successfully removed</returns>
        public static bool RemoveApplicationFromStartup()
        {
            RegistryKey rk;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (appPath == null)
                {
                    rk.DeleteValue(appName);
                }
                else
                {
                    if (rk.GetValue(appName).ToString().ToLower() == appPath.ToLower())
                    {
                        rk.DeleteValue(appName);
                    }
                }
                return true;
            }
            catch (Exception)
            {
            }

            try
            {
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (appPath == null)
                {
                    rk.DeleteValue(appName);
                }
                else
                {
                    if (rk.GetValue(appName).ToString().ToLower() == appPath.ToLower())
                    {
                        rk.DeleteValue(appName);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}