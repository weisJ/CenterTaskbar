namespace CenterTaskbar
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    /// <summary>
    /// Defines the <see cref="Resizer" />
    /// </summary>
    internal static class Resizer
    {
        /// <summary>
        /// Resize the given taskbar to be centered.
        /// </summary>
        /// <param name="taskbar">Taskbar to center</param>
        /// <param name="framerate">framerate of monitor</param>
        /// <param name="force">The force<see cref="bool"/></param>
        /// <returns>whether the taskbar had to be resized. If the resizing happened and performed without issue.</returns>
        public static bool Resize(Taskbar taskbar, int framerate, bool force)
        {
            Debug.Print("Starting Resize");
            if (!taskbar.UpdateLastElementPos())
            {
                //size/location unchanged, sleeping
                return true;
            }
            Debug.Print("Updated Last Pos");
            (double itemListSize, double traySize, int numOfItems) = taskbar.GetSizes();

            if (Tools.AreSimilar(itemListSize, 0) && !force)
            {
                return true;
            }

            (Rect trayBounds, Rect taskBounds) = taskbar.GetBounds();

            Debug.Print("TrayBounds: " + trayBounds);
            Debug.Print("TaskBounds: " + taskBounds);
            Debug.Print("itemListSize: " + itemListSize);
            Debug.Print("traySize: " + traySize);

            double targetPos = Math.Round((traySize - itemListSize) / 2)
               + (taskbar.IsHorizontal() ? trayBounds.X : trayBounds.Y);

            Debug.Print("TargetPos: " + targetPos);

            double rightBounds = taskbar.EndListBoundary();
            if ((targetPos + itemListSize) > (rightBounds))
            {
                // Shift off center when the bar is too big
                double extra = (targetPos + itemListSize) - rightBounds;
                Debug.WriteLine("Shifting off center, too big and hitting right/bottom boundary (" + (targetPos + itemListSize) + " > " + rightBounds + ") // " + extra);
                targetPos -= extra;
            }

            double beginBound = taskbar.BeginListBoundary();
            if (targetPos < beginBound)
            {
                // Prevent X position ending up beyond the normal left aligned position
                Debug.WriteLine("Target is more left than left/top aligned default, left/top aligning (" + targetPos + " <= " + beginBound + ")");
                taskbar.Reset();
                return true;
            }

            targetPos = NewPosition(targetPos, beginBound);

            double delta = Math.Abs(targetPos - (taskbar.IsHorizontal() ? taskbar.X : taskbar.Y));
            if (delta <= 1 && !force)
            {
                // Already positioned within margin of error, avoid the unneeded MoveWindow call
                Debug.WriteLine("Already positioned, ending to avoid the unneeded MoveWindow call (Delta: " + delta + ")");
                return true;
            }

            Move(taskbar, (int)targetPos, traySize, framerate);
            return true;
        }

        /// <summary>
        /// Calculate the new Position of the tasklist.
        /// </summary>
        /// <param name="targetPos">Pre-calculated target position</param>
        /// <param name="beginBound">beginning bound of the tasklist</param>
        /// <returns></returns>
        private static double NewPosition(double targetPos, double beginBound)
        {
            double newPos = targetPos - beginBound;
            if (newPos < 0)
            {
                Debug.WriteLine("Relative position < 0, adjusting to 0 (Previous: " + newPos + ")");
                newPos = 0;
            }
            return newPos;
        }

        /// <summary>
        /// Move the tasklist to the specified position
        /// </summary>
        /// <param name="taskbar">Taskbar to move the tasklist of</param>
        /// <param name="position">Position to move to</param>
        /// <param name="traySize">Size of tray</param>
        /// <param name="framerate">framerate of monitor</param>
        private static void Move(Taskbar taskbar, int position, double traySize, int framerate)
        {
            if (taskbar.IsHorizontal())
            {
                Debug.Print("Moving to: " + position);
                taskbar.SetPosition(position, 0);
                Debug.Print("Finished Moving");
            }
            else
            {
                Debug.Print("Moving to: " + position);
                taskbar.SetPosition(0, position);
                Debug.Print("Finished Moving");
            }
        }
    }
}