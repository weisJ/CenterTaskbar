using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CenterTaskbar
{
    class Resizer
    {
        /// <summary>
        /// Resize the given taskbar to be centred.
        /// </summary>
        /// <param name="taskbar">Taskbar to center</param>
        /// <returns>whther the taskbar had to be resized. If the resizing happened and perfomed without issue.</returns>
        public static Boolean Resize(Taskbar taskbar)
        {
            if (!taskbar.UpdateLastElementPos())
            {
                //Size/location unchanged, sleeping
                return false;
            }

            (double itemListSize, double traySize) = taskbar.GetSizes();
            (Rect trayBounds, Rect taskBounds) = taskbar.GetBounds();

            double targetPos = Math.Round((traySize - itemListSize) / 2)
               + (taskbar.IsHorizontal() ? trayBounds.X : trayBounds.Y);

            double delta = Math.Abs(targetPos - (taskbar.IsHorizontal() ? taskBounds.X : taskBounds.Y));
            if (delta <= 1)
            {
                // Already positioned within margin of error, avoid the unneeded MoveWindow call
                Debug.WriteLine("Already positioned, ending to avoid the unneeded MoveWindow call (Delta: " + delta + ")");
                return false;
            }

            double rightBounds = taskbar.RightListBoundary();
            if ((targetPos + itemListSize) > (rightBounds))
            {
                // Shift off center when the bar is too big
                double extra = (targetPos + itemListSize) - rightBounds;
                Debug.WriteLine("Shifting off center, too big and hitting right/bottom boundary (" + (targetPos + itemListSize) + " > " + rightBounds + ") // " + extra);
                targetPos -= extra;
            }

            double leftBounds = taskbar.LeftListBoundary();
            if (targetPos <= leftBounds)
            {
                // Prevent X position ending up beyond the normal left aligned position
                Debug.WriteLine("Target is more left than left/top aligned default, left/top aligning (" + targetPos + " <= " + leftBounds + ")");
                taskbar.Reset();
                return true;
            }
            Move(taskbar, (int)NewPosition(targetPos, leftBounds), traySize);
            return true;
        }

        /// <summary>
        /// Calculate the new Position of the tasklist.
        /// </summary>
        /// <param name="targetPos">Precalculated target position</param>
        /// <param name="leftBounds">LeftBound of the tasklist</param>
        /// <returns></returns>
        private static double NewPosition(double targetPos, double leftBounds)
        {
            double newPos = targetPos - leftBounds;
            if (newPos < 0)
            {
                Debug.WriteLine("Relative position < 0, adjusting to 0 (Previous: " + newPos + ")");
                newPos = 0;
            }
            return newPos;
        }

        /// <summary>
        /// Move the tasklist to the spceified position
        /// </summary>
        /// <param name="taskbar">Taskbar to move the takslist of</param>
        /// <param name="position">Position to move to</param>
        /// <param name="traySize">Size of tray</param>
        private static void Move(Taskbar taskbar, int position, double traySize)
        {
            if (taskbar.IsHorizontal())
            {
                if (Math.Abs(position - taskbar.X) > traySize / 2)
                {
                    return;
                }
                taskbar.SetPosition(position, 0);
            }
            else
            {
                if (Math.Abs(position - taskbar.Y) > traySize / 2)
                {
                    return;
                }
                taskbar.SetPosition(0, position);
            }
        }
    }
}
