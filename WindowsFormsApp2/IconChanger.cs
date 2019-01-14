namespace CenterTaskbar
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="IconManager" />
    /// </summary>
    internal static class IconChanger
    {
        /// <summary>
        /// Threshold for which resolution to decide whether to use large taskbar icons.
        /// </summary>
        private const int BIG_TASKBAR_RES = 1920;

        /// <summary>
        /// Null pointer constant.
        /// </summary>
        private const int NULL = 0;

        /// <summary>
        /// Message Code for WINDOW Broadcast.
        /// </summary>
        private const int HWND_BROADCAST = 0xffff;

        /// <summary>
        /// Message Code for changed settings.
        /// </summary>
        private const int WM_SETTINGCHANGE = 0x001a;

        /// <summary>
        /// RegistryKey for small taskbar icons.
        /// </summary>
        private const string REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        /// <summary>
        /// RegistryName for small taskbar icons.
        /// </summary>
        private const string REG_NAME = "TaskbarSmallIcons";

        /// <summary>
        /// The IsTaskbarSmall
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        private static bool IsTaskbarSmall()
        {
            try
            {
                object value = null;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY, true))
                {
                    if (key != null)
                    {
                        value = key.GetValue(REG_NAME, 0);
                    }
                }
                if (value != null && value.GetType() == typeof(int))
                {
                    return (int)value != 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The SystemEvents_DisplaySettingsChanged
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        public static void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            try
            {
                bool small = IsTaskbarSmall(); // Can throw exceptions, abort in that case

                Debug.Print("Taskbar is currently " + (small ? "small" : "big"));

                int screenWidth = Screen.PrimaryScreen.Bounds.Width;

                // This resolution takes into account Windows' DPI setting, so even if a small 13 inch screen's native resolution is 1920,
                // if the DPI setting is set for example to 150% (which is a common thing), it returns 1280
                if (screenWidth < BIG_TASKBAR_RES && !small) // Update taskbar size to small if necessary
                {
                    UpdateTaskbarSize(true);
                }
                else if (screenWidth >= BIG_TASKBAR_RES && small) // Update taskbar size to big if necessary
                {
                    UpdateTaskbarSize(false);
                }
            } catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// The ToggleTaskbarSize
        /// </summary>
        private static void ToggleTaskbarSize()
        {
            bool small = IsTaskbarSmall();
            UpdateTaskbarSize(!small);
        }

        /// <summary>
        /// The UpdateTaskbarSize
        /// </summary>
        /// <param name="small">The small<see cref="bool"/></param>
        private static void UpdateTaskbarSize(bool small)
        {
            Debug.Print("Updating Taskbar to be " + (small ? "small" : "big"));
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY, true))
                {
                    if (key != null)
                    {
                        key.SetValue(REG_NAME, small ? 1 : 0);
                        NativeMethods.SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)NULL, "TraySettings");
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}