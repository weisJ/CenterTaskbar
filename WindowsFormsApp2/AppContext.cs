namespace CenterTaskbar
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Windows.Automation;
    using System.Windows.Forms;

    /// <summary>
    /// The ApplicationContext of the program
    /// </summary>
    internal class AppContext : ApplicationContext
    {
        /// <summary>
        /// Name of the application
        /// </summary>
        public const string AppName = "CenterTaskbar";

        /// <summary>
        /// Desktop reference.
        /// </summary>
        private readonly AutomationElement desktop = AutomationElement.RootElement;

        /// <summary>
        /// Current framerate of the system.
        /// </summary>
        private readonly int activeFramerate = 60;

        /// <summary>
        /// System tray icon of the application.
        /// </summary>
        private NotifyIcon trayIcon;

        /// <summary>
        /// HelperClass to add program to startup.
        /// </summary>
        private StartupHelper startupHelper;

        /// <summary>
        /// TrayResizer. Responsible for centering the taskbar icons.
        /// </summary>
        private TaskbarResizer trayResizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppContext"/> class.
        /// </summary>
        /// <param name="args">System arguments (first one is framerate)</param>
        public AppContext(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    activeFramerate = int.Parse(args[0]);
                    Debug.WriteLine("Active refresh rate: " + activeFramerate);
                }
                catch (FormatException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            startupHelper = new StartupHelper(AppName, activeFramerate);
            trayResizer = new TaskbarResizer();
            SetupTrayIcon();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            if(Properties.Settings.Default.changeIcons)
            {
                SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged_IconChange;
            }

            Start();
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public void Exit(object sender, EventArgs e)
        {
            Debug.Print("Exit Application");

            // Hide tray icon and dispose it, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            trayIcon.Dispose();

            trayResizer.ResetTaskbars();
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged_IconChange;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

            Environment.Exit(0);
        }

        /// <summary>
        /// Restart the centering process
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public void Restart(object sender, EventArgs e)
        {
            Debug.Print("Restarting");
            Start();
        }

        /// <summary>
        /// Reload the application to detect new taskbars.
        /// </summary>
        /// <param name="sender">message sender</param>
        /// <param name="e">event arguments</param>
        public void Reload(object sender, EventArgs e)
        {
            Debug.Print("Reloading");
            trayResizer.IsInitialized = false;
            Start();
        }

        /// <summary>
        /// Start centering the taskbar
        /// </summary>
        public void Start()
        {
            Debug.Print("Starting");
            if (!trayResizer.IsInitialized)
            {
                trayResizer.InitTaskbars(OnUIAutomationEvent, activeFramerate);
            }

            trayResizer.ForceResizeTaskbars();
        }

        /// <summary>
        /// Method to handle SystemEvents_DisplaySettingsChanged Events.
        /// </summary>
        /// <param name="sender">sender<see cref="object"/></param>
        /// <param name="e">event arguments<see cref="EventArgs"/></param>
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("The display settings changed.");
            Reload(null, null);
        }

        /// <summary>
        /// Method to handle SystemEvents_DisplaySettingsChanged Events.
        /// </summary>
        /// <param name="sender">sender<see cref="object"/></param>
        /// <param name="e">event arguments<see cref="EventArgs"/></param>
        private void SystemEvents_UserPreferenceChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("User Preference changed.");
            Debug.Print("Category: " + (e as UserPreferenceChangedEventArgs).Category.ToString());
            Reload(null, null);
        }

        /// <summary>
        /// Method to handle SystemEvents_DisplaySettingsChanged Events.
        /// </summary>
        /// <param name="sender">sender<see cref="object"/></param>
        /// <param name="e">event arguments<see cref="EventArgs"/></param>
        private void SystemEvents_DisplaySettingsChanged_IconChange(object sender, EventArgs e)
        {
            Debug.WriteLine("IconChanger active");
            IconChanger.SystemEvents_DisplaySettingsChanged(sender, e);

            //Reload so that execution order doesn't matter.
            Reload(null, null);
        }

        /// <summary>
        /// Setup the system tray icon.
        /// </summary>
        private void SetupTrayIcon()
        {
            MenuItem startup = new MenuItem("Start with Windows", startupHelper.ToggleStartup)
            {
                Checked = startupHelper.IsApplicationInStatup()
            };

            MenuItem changeIcons = new MenuItem("Automatically change icon size", ToggleChangeIcons)
            {
                Checked = Properties.Settings.Default.changeIcons
            };

            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Icon1,
                Text = "CenterTaskbar (" + activeFramerate + ")",
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Scan for screens", Reload),
                    startup,
                    changeIcons,
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        /// <summary>
        /// Toggle the changeIcons setting.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event arguments</param>
        private void ToggleChangeIcons(object sender, EventArgs e)
        {
            if(Properties.Settings.Default.changeIcons)
            {
                Debug.Print("Turned off auto change icons");
                Properties.Settings.Default.changeIcons = false;
                (sender as MenuItem).Checked = false;

                SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged_IconChange;
            } else
            {
                Debug.Print("Turned on auto change icons");
                Properties.Settings.Default.changeIcons = true;
                (sender as MenuItem).Checked = true;

                SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged_IconChange;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Start the loop if the program went to sleep at taskbar event.
        /// Start the loop if the program went to sleep at taskbar event.
        /// </summary>
        /// <param name="src">Event source</param>
        /// <param name="e">Event Arguments</param>
        private void OnUIAutomationEvent(object src, AutomationEventArgs e)
        {
            try
            {
                trayResizer.ForceResizeTaskbars();
                trayResizer.ForceResizeTaskbars();
            }
            catch (Exception ex)
            {
                Debug.Print("Exception: " + ex.Message);
                try
                {
                    trayResizer.ForceResizeTaskbars();
                }
                catch (Exception ex2)
                {
                    Debug.Print("Exception: " + ex2.Message);
                }
            }
        }
    }
}