namespace CenterTaskbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows.Automation;

    /// <summary>
    /// Defines the <see cref="TaskbarResizer" />
    /// </summary>
    internal class TaskbarResizer
    {
        /// <summary>
        /// Attribute name of taskbar tray.
        /// </summary>
        private const String Shell_TrayWnd = "Shell_TrayWnd";

        /// <summary>
        /// Attribute name of secondary taskbar tray.
        /// </summary>
        private const String Shell_SecondaryTrayWnd = "Shell_SecondaryTrayWnd";

        /// <summary>
        /// Desktop object.
        /// </summary>
        private readonly AutomationElement desktop = AutomationElement.RootElement;

        /// <summary>
        /// current system framerate.
        /// </summary>
        private int framerate;

        /// <summary>
        /// List of taskbars active.
        /// </summary>
        public List<Taskbar> Taskbars { get; private set; } = new List<Taskbar>();

        /// <summary>
        /// Indicates whether the taskbars have been initialized.
        /// After initialization to recognize new taskbars the program has to be reloaded.
        /// </summary>
        public bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Initialize all Taskbars with the AutomationEvent.
        /// </summary>
        /// <param name="onUIAutomationEvent">AutomationEvent to perform at change</param>
        /// <param name="framerate">framerate of monitor</param>
        public void InitTaskbars(Action<object, AutomationEventArgs> onUIAutomationEvent, int framerate)
        {
            this.framerate = framerate;
            OrCondition isInTrayCondition = new OrCondition(
                new PropertyCondition(AutomationElement.ClassNameProperty, Shell_TrayWnd),
                new PropertyCondition(AutomationElement.ClassNameProperty, Shell_SecondaryTrayWnd));
            CacheRequest cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);

            Taskbars.Clear();

            using (cacheRequest.Activate())
            {
                AutomationElementCollection trayList = desktop.FindAll(TreeScope.Children, isInTrayCondition);
                Debug.Assert(trayList != null, "Null values found, aborting (There are no trays)");
                Debug.Print(trayList.Count + " trays(s) detected");

                foreach (AutomationElement tray in trayList)
                {
                    Taskbar taskbar = new Taskbar(tray);
                    //taskbar.AddEventHandler(onUIAutomationEvent);
                    Taskbars.Add(taskbar);
                }
            }

            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                desktop,
                TreeScope.Subtree,
                (object src, AutomationEventArgs e) => onUIAutomationEvent.Invoke(src, e));
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowClosedEvent,
                desktop, TreeScope.Subtree,
                (object src, AutomationEventArgs e) => onUIAutomationEvent.Invoke(src, e));

            IsInitialized = true;
        }

        /// <summary>
        /// Resizes all Taskbars.
        /// </summary>
        /// <returns>Number of Taskbars that have been resized</returns>
        public int ResizeTaskbars()
        {
            return ResizeTaskbars(false);
        }

        /// <summary>
        /// The ForceResizeTaskbars
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int ForceResizeTaskbars()
        {
            return ResizeTaskbars(true);
        }

        /// <summary>
        /// The ResizeTaskbars
        /// </summary>
        /// <param name="force">The force<see cref="bool"/></param>
        /// <returns>The <see cref="int"/></returns>
        public int ResizeTaskbars(bool force)
        {
            //Debug.Print("Starting Resize (force=" + force + ")");
            int runs = 0;
            foreach (Taskbar taskbar in Taskbars)
            {
                Debug.Print("Reloading Tasks");
                taskbar.ReloadTaskList();
                Debug.Print("Done");

                if (Resizer.Resize(taskbar, framerate, force))
                {
                    runs++;
                }
                else
                {
                    Resizer.Resize(taskbar, framerate, true);
                }
            }
            return runs;
        }

        /// <summary>
        /// Reset all taskbars
        /// </summary>
        public void ResetTaskbars()
        {
            foreach (Taskbar taskbar in Taskbars)
            {
                taskbar.Reset();
            }
        }
    }
}