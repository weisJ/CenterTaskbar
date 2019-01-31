namespace CenterTaskbar
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Automation;

    /// <summary>
    /// Defines the <see cref="Taskbar" />
    /// </summary>
    internal class Taskbar
    {
        /// <summary>
        /// NameAtribute of tasklist.
        /// </summary>
        private const string MSTaskListWClass = "MSTaskListWClass";

        /// <summary>
        /// uFlag for SetWindowPos.
        /// </summary>
        private const int SWP_NOSIZE = 0x0001;

        /// <summary>
        /// uFlag for SetWindowPos.
        /// </summary>
        private const int SWP_NOZORDER = 0x0004;

        /// <summary>
        /// uFlag for SetWindowPos.
        /// </summary>
        private const int SWP_SHOWWINDOW = 0x0040;

        /// <summary>
        /// uFlag for SetWindowPos.
        /// </summary>
        private const int SWP_ASYNCWINDOWPOS = 0x4000;

        /// <summary>
        /// SystemTray object.
        /// </summary>
        private readonly AutomationElement tray;

        /// <summary>
        /// tasklist object.
        /// </summary>
        private AutomationElement tasks;

        /// <summary>
        /// taskBuffer for when it is currently null.
        /// </summary>
        private AutomationElement taskBuffer;

        /// <summary>
        /// beginBoundaryBuffer and endBoundaryBuffer
        /// </summary>
        private double beginBoundaryBuffer, endBoundaryBuffer;

        /// <summary>
        /// x and y position
        /// </summary>
        private double x, y;

        /// <summary>
        /// Gets or sets the X
        /// </summary>
        public double X
        {
            get => x;

            set
            {
                x = value;
                Debug.Print("New X: " + x);
            }
        }

        /// <summary>
        /// Gets or sets the Y
        /// </summary>
        public double Y
        {
            get => y;

            set
            {
                y = value;
                Debug.Print("New X: " + y);
            }
        }

        /// <summary>
        /// Position of the lastElement in the taskList.
        /// </summary>
        private double lastPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Taskbar"/> class.
        /// </summary>
        /// <param name="tray">The tray<see cref="AutomationElement"/></param>
        public Taskbar(AutomationElement tray)
        {
            this.tray = tray;
            lastPosition = 0;
            ReloadTaskList();
            GetLastTaskElement();
            GetFirstTaskElement();

            Debug.Print("Taskbar: " + tray.ToString() + " | Tasklist: " + tasks.ToString());
        }

        /// <summary>
        /// Reload the tasklist
        /// </summary>
        public void ReloadTaskList()
        {
            if (tasks == null)
            {
                CacheRequest cacheRequest = new CacheRequest();
                cacheRequest.Add(AutomationElement.NameProperty);
                using (cacheRequest.Activate())
                {
                    tasks = tray.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ClassNameProperty, MSTaskListWClass));
                    if (tasks != null)
                    {
                        taskBuffer = tasks;
                        //NativeMethods.SetParent((IntPtr)tasks.Current.NativeWindowHandle, (IntPtr)tray.Current.NativeWindowHandle);
                    }
                }
            }
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

            if (Tools.AreSimilar(lastElementPos, lastPosition))
            {
                return false;
            }
            lastPosition = lastElementPos;
            return true;
        }

        /// <summary>
        /// Calculate the size of the tray and the itemList
        /// </summary>
        /// <returns>(itemListSize, traySize, numOfItems)</returns>
        public (double itemlistSize, double traySize, int numOfItems) GetSizes()
        {
            AutomationElement lastElement = GetLastTaskElement();
            AutomationElement firstElement = GetFirstTaskElement();
            Debug.Assert(lastElement != null && firstElement != null, "Last/First Element in " + tray + " is null");

            if (tasks == null)
            {
                tasks = taskBuffer;
            }
            if (taskBuffer == null)
            {
                throw new NullReferenceException();
            }

            Rect trayBounds = tray.Current.BoundingRectangle;
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
        /// <returns>(itemListSize, traySize, numOfItems)</returns>
        private (double itemlistSize, double traySize, int numOfItems) CalculateSizes(AutomationElement firstElement, AutomationElement lastElement, Rect trayBounds, Rect taskBounds)
        {
            bool horizontal = IsHorizontal();

            double scale = horizontal
              ? lastElement.Current.BoundingRectangle.Height / trayBounds.Height
              : lastElement.Current.BoundingRectangle.Width / trayBounds.Width;
            double itemlistSize = (horizontal
                ? lastElement.Current.BoundingRectangle.Left - firstElement.Current.BoundingRectangle.Left
                : lastElement.Current.BoundingRectangle.Top - firstElement.Current.BoundingRectangle.Top) / scale;

            if(itemlistSize < 0)
            {
                throw new Exception("ItemlistSIze is < 0");
            }

            double traySize = horizontal
                ? tray.Cached.BoundingRectangle.Width
                : tray.Cached.BoundingRectangle.Height;

            int numOfItems = (int)(itemlistSize / (horizontal
                ? firstElement.Current.BoundingRectangle.Width
                : firstElement.Current.BoundingRectangle.Height));

            return (itemlistSize, traySize, numOfItems);
        }

        /// <summary>
        /// Get the beginning Boundary position of the tasklist.
        /// If the tasklist is currently null an cached version is returned.
        /// </summary>
        /// <returns>beginning boundary of tasklist</returns>
        public double BeginListBoundary()
        {
            beginBoundaryBuffer = SideBoundary(true);
            return beginBoundaryBuffer;
        }

        /// <summary>
        /// Get the ending Boundary position of the tasklist.
        /// If the tasklist is currently null an cached version is returned.
        /// </summary>
        /// <returns>ending boundary of tasklist</returns>
        public double EndListBoundary()
        {
            endBoundaryBuffer = SideBoundary(false);
            return endBoundaryBuffer;
        }

        /// <summary>
        /// Get the boundary distance of specified side of the tasklist.
        /// </summary>
        /// <param name="begin">true = beginning, false = ending e.g. for horizontal taskbar: true = left side, false = right side</param>
        /// <returns></returns>
        private double SideBoundary(bool begin)
        {
            if (tasks == null)
            {
                return begin ? beginBoundaryBuffer : endBoundaryBuffer;
            }

            bool horizontal = IsHorizontal();
            double adjustment = 0;
            AutomationElement prevSibling = TreeWalker.ControlViewWalker.GetPreviousSibling(tasks);
            AutomationElement nextSibling = TreeWalker.ControlViewWalker.GetNextSibling(tasks);
            AutomationElement parent = TreeWalker.ControlViewWalker.GetParent(tasks);

            if ((begin && prevSibling != null))
            {
                adjustment = (horizontal
                    ? prevSibling.Current.BoundingRectangle.Right
                    : prevSibling.Current.BoundingRectangle.Bottom);
            }
            else if (!begin && nextSibling != null)
            {
                adjustment = horizontal
                    ? nextSibling.Current.BoundingRectangle.Left
                    : nextSibling.Current.BoundingRectangle.Top;
            }
            else if (parent != null)
            {
                adjustment = horizontal
                    ? begin
                        ? parent.Current.BoundingRectangle.Left
                        : parent.Current.BoundingRectangle.Right
                    : begin
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
        public void Reset()
        {
            Debug.Print("Reseting Position");
            ReloadTaskList();
            SetPosition(0, 0, false);
        }

        /// <summary>
        /// Set the position of the tasklist.
        /// </summary>
        /// <param name="x">x Position</param>
        /// <param name="y">y Position</param>
        public void SetPosition(int x, int y)
        {
            SetPosition(x, y, true);
        }

        /// <summary>
        /// Set the position of the tasklist.
        /// </summary>
        /// <param name="xPos">x Position</param>
        /// <param name="yPos">y Position</param>
        /// <param name="updatePos">whether the new position should be saved to X and Y coordinates</param>
        private void SetPosition(int xPos, int yPos, bool updatePos)
        {
            if (tasks == null)
            {
                return; //Nothing needs to be done.
            }

            IntPtr tasklistPtr = (IntPtr)tasks.Current.NativeWindowHandle;
            NativeMethods.SetWindowPos(tasklistPtr, IntPtr.Zero, xPos, yPos, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_ASYNCWINDOWPOS);

            Rect bounds = tasks.Current.BoundingRectangle;
            Debug.Print("Now Position: " + bounds.X);

            if (updatePos)
            {
                x = xPos;
                y = yPos;
            }

            AutomationElement lastElement = TreeWalker.ControlViewWalker.GetLastChild(tasks);
            if (lastElement != null)
            {
                lastPosition = IsHorizontal()
                    ? lastElement.Current.BoundingRectangle.Left
                    : lastElement.Current.BoundingRectangle.Top;
            }
        }

        /// <summary>
        /// Returns the first Element in the tasklist.
        /// If the tasklist is currently null an cached element is returned.
        /// </summary>
        /// <returns>the first element in tasklist</returns>
        private AutomationElement GetFirstTaskElement()
        {
            if (tasks == null)
            {
                if (taskBuffer == null)
                {
                    throw new NullReferenceException("tasks is null");
                }
                return TreeWalker.ContentViewWalker.GetFirstChild(taskBuffer);
            }
            return TreeWalker.ContentViewWalker.GetFirstChild(tasks);
        }

        /// <summary>
        /// Returns the last Element in the tasklist.
        /// If the tasklist is currently null an cached element is returned.
        /// </summary>
        /// <returns>the last element in tasklist</returns>
        private AutomationElement GetLastTaskElement()
        {
            if (tasks == null)
            {
                if (taskBuffer == null)
                {
                    throw new NullReferenceException("tasks is null");
                }
                return TreeWalker.ContentViewWalker.GetLastChild(taskBuffer);
            }
            return TreeWalker.ContentViewWalker.GetLastChild(tasks);
        }
    }
}