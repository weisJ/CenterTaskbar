using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Automation;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Security.Permissions;

namespace CenterTaskbar
{
    static class Program
    {
        private static CustomApplicationContext customAppContext;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private class TestMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == /*WM_CLOSE*/ 0x10 && customAppContext != null)
                {
                    customAppContext.Exit(null, null);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new TestMessageFilter());

            customAppContext = new CustomApplicationContext(args);
            Application.Run(customAppContext);
        }
    }

    public class CustomApplicationContext : ApplicationContext
    {
        public const String appName = "CenterTaskbar";

        private readonly AutomationElement desktop = AutomationElement.RootElement;

        private readonly int activeFramerate = 60;

        private bool running = false;

        private NotifyIcon trayIcon;
        private StartupHelper startupHelper;
        private Thread positionThread;
        private TaskbarResizer trayResizer;

        public CustomApplicationContext(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    activeFramerate = Int32.Parse(args[0]);
                    Debug.WriteLine("Active refresh rate: " + activeFramerate);
                }
                catch (FormatException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            startupHelper = new StartupHelper(appName, activeFramerate);
            trayResizer = new TaskbarResizer();
            SetupTrayIcon();

            Start();
        }

        /// <summary>
        /// Setup the system tray icon.
        /// </summary>
        private void SetupTrayIcon()
        {
            MenuItem header = new MenuItem("CenterTaskbar (" + activeFramerate + ")", Exit)
            {
                Enabled = false
            };
            MenuItem startup = new MenuItem("Start with Windows", startupHelper.ToggleStartup)
            {
                Checked = startupHelper.IsApplicationInStatup()
            };

            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Icon1,
                ContextMenu = new ContextMenu(new MenuItem[] {
                header,
                new MenuItem("Scan for screens", Restart),
                startup,
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
        }

        /// <summary>
        /// Exit the aplication
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Stop();

            trayResizer.ResetTaskbars();

            if (Application.MessageLoop)
            {
                // WinForms app
                Application.Exit();
            }
            else
            {
                // Console app
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Restart the centering process
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        void Restart(object sender, EventArgs e)
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Start centering the taskbar
        /// </summary>
        private void Start()
        {
            trayResizer.InitTaskbars(OnUIAutomationEvent);
            running = true;
            Loop();
        }

        /// <summary>
        /// Stop centering the taskbar
        /// </summary>
        private void Stop()
        {
            running = false;
            if (positionThread != null)
            {
                positionThread.Join();
                positionThread = null;
            }
        }


        /// <summary>
        /// Start the loop if the program went to sleep at taskbar event.
        /// </summary>
        /// <param name="src">Event source</param>
        /// <param name="e">Event Arguments</param>
        private void OnUIAutomationEvent(object src, AutomationEventArgs e)
        {
            if (positionThread != null && !positionThread.IsAlive)
            {
                Loop();
            }
        }

        /// <summary>
        /// Loop that controls the resizing of the taskbar. If nothing happens the loop will go to sleep.
        /// </summary>
        private void Loop()
        {
            positionThread = new Thread(() =>
            {
                int runs = 0;
                while (runs < (activeFramerate / 5) && running)
                {
                    runs += trayResizer.ResizeTaskbars();
                    Thread.Sleep(1000 / activeFramerate);
                }
                Debug.WriteLine("Thread ended due to inactivity, sleeping");
            });
            positionThread.Start();
        }

    }
}
