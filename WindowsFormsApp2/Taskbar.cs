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
    class Taskbar
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);


        private const String MSTaskListWClass = "MSTaskListWClass";
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;

        private readonly AutomationElement tray;
        private AutomationElement tasks;
        private AutomationElement firstElementBuffer, lastElementBuffer = null;
        private double leftBoundaryBuffer, rightBOundaryBuffer;

        public double X { get; private set; }
        public double Y { get; private set; }

        //Position of the lastElement in the taskList.
        private double lastPosition;

        public Taskbar(AutomationElement tray)
        {
            this.tray = tray;
            this.lastPosition = 0;
            ReloadTaskList();
            GetLastTaskElement();
            GetFirstTaskElement();
        }

        /// <summary>
        /// Reload the tasklist
        /// </summary>
        public void ReloadTaskList()
        {
            CacheRequest cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.NameProperty);
            using (cacheRequest.Activate())
            {
                this.tasks = tray.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ClassNameProperty, MSTaskListWClass));
            }
            Debug.Print(tasks.ToString());
        }

        /// <summary>
        /// Add an AutomationPropertyChangedEventHandler to the taskbar
        /// </summary>
        /// <param name="onUIAutomationEvent">The Handler to add</param>
        public void AddEventHandler(Action<object, AutomationEventArgs> onUIAutomationEvent)
        {
            Automation.AddAutomationPropertyChangedEventHandler(
                tasks,
                TreeScope.Element,
                new AutomationPropertyChangedEventHandler(onUIAutomationEvent),
                AutomationElement.BoundingRectangleProperty);
        }

        /// <summary>
        /// Update the position of the lastElement in the itemList.
        /// </summary>
        /// <returns>true if value has changed</returns>
        public bool UpdateLastElementPos()
        {
            AutomationElement lastElement = GetLastTaskElement();
            bool horizontal = IsHorizontal();
            // Use the left/top bounds because there is an empty element as the last child with a nonzero width
            double lastElementPos = horizontal
                ? lastElement.Current.BoundingRectangle.Left
                : lastElement.Current.BoundingRectangle.Top;

            if (AreSimilar(lastElementPos, lastPosition))
            {
                return false;
            }
            lastPosition = lastElementPos;
            return true;
        }

        /// <summary>
        /// Calculate the size of the tray and the itemList
        /// </summary>
        /// <returns>(itemListSize, traySize)</returns>
        public (double itemlistSize, double traySize) GetSizes()
        {
            AutomationElement lastElement = GetLastTaskElement();
            AutomationElement firstElement = GetFirstTaskElement();
            Debug.Assert(lastElement != null && firstElement != null, "Last/First Element in " + tray + " is null");

            Rect trayBounds = tray.Cached.BoundingRectangle;
            Rect taskBounds = tasks.Current.BoundingRectangle;

            return CalculateSizes(firstElement, lastElement, trayBounds, taskBounds);
        }

        /// <summary>
        /// Returns the bounds of the tray and taskList.
        /// </summary>
        /// <returns>(trayBounds, tasksBounds)</returns>
        public (Rect trayBounds, Rect taskBounds) GetBounds()
        {
            Rect trayBounds = tray.Cached.BoundingRectangle;
            Rect taskBounds = tasks.Current.BoundingRectangle;
            return (trayBounds, taskBounds);
        }

        /// <summary>
        /// Calculate the sizes of tray and itemList
        /// </summary>
        /// <param name="firstElement">first element in tasks</param>
        /// <param name="lastElement">last element in tasks</param>
        /// <param name="trayBounds">bounds of tray</param>
        /// <param name="taskBounds">bounds of tasks</param>
        /// <returns>(itemListSize, traySize)</returns>
        private (double itemlistSize, double traySize) CalculateSizes(AutomationElement firstElement, AutomationElement lastElement, Rect trayBounds, Rect taskBounds)
        {
            bool horizontal = IsHorizontal();

            double scale = horizontal
              ? lastElement.Current.BoundingRectangle.Height / trayBounds.Height
              : lastElement.Current.BoundingRectangle.Width / trayBounds.Width;
            double itemlistSize = lastPosition - (horizontal
                ? firstElement.Current.BoundingRectangle.Left
                : firstElement.Current.BoundingRectangle.Top) / scale;
            Debug.Assert(itemlistSize > 0);

            double traySize = horizontal
                ? tray.Cached.BoundingRectangle.Width
                : tray.Cached.BoundingRectangle.Height;

            return (itemlistSize, traySize);
        }

        /// <summary>
        /// Get the left Boundary position of the tasklist.
        /// </summary>
        /// <returns>left boundary of tasklist</returns>
        public double LeftListBoundary()
        {
            leftBoundaryBuffer = SideBoundary(true);
            return leftBoundaryBuffer;
        }


        /// <summary>
        /// Get the right Boundary position of the tasklist.
        /// </summary>
        /// <returns>right boundary of tasklist</returns>
        public double RightListBoundary()
        {
            rightBOundaryBuffer = SideBoundary(false);
            return rightBOundaryBuffer;
        }

        /// <summary>
        /// Get the boundary distance of specified side of the tasklist.
        /// </summary>
        /// <param name="left">true = left side, false = right side</param>
        /// <returns></returns>
        private double SideBoundary(bool left)
        {
            if (tasks == null)
            {
                return left ? leftBoundaryBuffer : rightBOundaryBuffer;
            }

            bool horizontal = IsHorizontal();
            double adjustment = 0;
            AutomationElement prevSibling = TreeWalker.ControlViewWalker.GetPreviousSibling(tasks);
            AutomationElement nextSibling = TreeWalker.ControlViewWalker.GetNextSibling(tasks);
            AutomationElement parent = TreeWalker.ControlViewWalker.GetParent(tasks);

            if ((left && prevSibling != null))
            {
                adjustment = (horizontal 
                    ? prevSibling.Current.BoundingRectangle.Right
                    : prevSibling.Current.BoundingRectangle.Bottom);
            }
            else if (!left && nextSibling != null)
            {
                adjustment = horizontal
                    ? nextSibling.Current.BoundingRectangle.Left
                    : nextSibling.Current.BoundingRectangle.Top;
            }
            else if (parent != null)
            {
                adjustment = horizontal
                    ? left
                        ? parent.Current.BoundingRectangle.Left
                        : parent.Current.BoundingRectangle.Right
                    : left
                        ? parent.Current.BoundingRectangle.Top
                        : parent.Current.BoundingRectangle.Bottom;
            }
            return adjustment;
        }

        /// <summary>
        /// Returns whether the taskbar is horizontal or vertical.
        /// </summary>
        /// <returns>true = horizontal, false = vertical</returns>
        public bool IsHorizontal()
        {
            Rect bounds = tray.Cached.BoundingRectangle;
            return bounds.Width > bounds.Height;
        }

        /// <summary>
        /// Reset the taskbar to its original position
        /// </summary>
        public void Reset() => SetPosition(0, 0);

        /// <summary>
        /// Set the position of the tasklist.
        /// </summary>
        /// <param name="x">x Position</param>
        /// <param name="y">y Position</param>
        public void SetPosition(int x, int y)
        {
            if (tasks == null)
            {
                return; //Nothing needs to be done.
            }

            IntPtr tasklistPtr = (IntPtr)tasks.Current.NativeWindowHandle;
            SetWindowPos(tasklistPtr, IntPtr.Zero, x, y, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_ASYNCWINDOWPOS);

            this.X = x;
            this.Y = y;

            AutomationElement lastElement = TreeWalker.ControlViewWalker.GetLastChild(tasks);
            lastPosition = IsHorizontal()
                ? lastElement.Current.BoundingRectangle.Left
                : lastElement.Current.BoundingRectangle.Top;
        }

        private AutomationElement GetFirstTaskElement()
        {
            if (tasks == null)
            {
                return firstElementBuffer;
            }
            AutomationElement firstElement = TreeWalker.ControlViewWalker.GetFirstChild(tasks);
            if (firstElement != null)
            {
                firstElementBuffer = firstElement;
            }
            return firstElementBuffer;
        }

        private AutomationElement GetLastTaskElement()
        {
            if (tasks == null)
            {
                return lastElementBuffer;
            }
            AutomationElement lastElement = TreeWalker.ControlViewWalker.GetLastChild(tasks);
            if (lastElement != null)
            {
                lastElementBuffer = lastElement;
            }
            return lastElementBuffer;
        }

        /// <summary>
        /// Checks whether two double values are approximaptly equal to accomodate floating point errors.
        /// </summary>
        /// <param name="a">first double</param>
        /// <param name="b">second double</param>
        /// <returns>true of the difference is in a margin of error.</returns>
        private static bool AreSimilar(double a, double b) => Math.Abs(a - b) < 0.00001;

    }
}
