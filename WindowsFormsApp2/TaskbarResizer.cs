using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;

namespace CenterTaskbar
{
    class TaskbarResizer
    {
        private const String Shell_TrayWnd = "Shell_TrayWnd";
        private const String Shell_SecondaryTrayWnd = "Shell_SecondaryTrayWnd";

        private readonly AutomationElement desktop = AutomationElement.RootElement;

        public List<Taskbar> Taskbars { get; private set; } = new List<Taskbar>();

        /// <summary>
        /// Initialize all Taskbars with the AutomationEvent.
        /// </summary>
        /// <param name="onUIAutomationEvent">AutomationEvent to perfom at change</param>
        public void InitTaskbars(Action<object, AutomationEventArgs> onUIAutomationEvent)
        {
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
                Debug.WriteLine(trayList.Count + " trays(s) detected");

                foreach (AutomationElement tray in trayList)
                {
                    Taskbar taskbar = new Taskbar(tray);
                    //taskbar.AddEventHandler(onUIAutomationEvent); //temporarily disabled
                    Taskbars.Add(new Taskbar(tray));
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

        }

        /// <summary>
        /// Resizes all Taskbars.
        /// </summary>
        /// <returns>Number of Taskbars that have been resized</returns>
        public int ResizeTaskbars()
        {
            int runs = 0;
            foreach (Taskbar taskbar in Taskbars)
            {
                taskbar.ReloadTaskList();
                if (Resizer.Resize(taskbar))
                {
                    runs++;
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
