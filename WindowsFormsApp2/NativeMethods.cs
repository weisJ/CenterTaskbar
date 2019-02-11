namespace CenterTaskbar
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Defines the <see cref="NativeMethods" />
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// SetWindowPosition
        /// </summary>
        /// <param name="hWnd">Handle to the window.<see cref="IntPtr"/></param>
        /// <param name="hWndInsertAfter">The hWndInsertAfter<see cref="IntPtr"/></param>
        /// <param name="X">Specifies the new position of the left side of the window, in client coordinates.<see cref="int"/></param>
        /// <param name="Y">Specifies the new position of the top of the window, in client coordinates.<see cref="int"/></param>
        /// <param name="cx">Specifies the new width of the window, in pixels.<see cref="int"/></param>
        /// <param name="cy">Specifies the new height of the window, in pixels.<see cref="int"/></param>
        /// <param name="uFlags">Specifies the window sizing and positioning flags. This parameter can be a combination of the following values.<see cref="int"/></param>
        /// <returns>whether position has successfully changed.<see cref="bool"/></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        /// <summary>
        /// SendNotifyMessage
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="Msg">MessageCode to send.</param>
        /// <param name="wParam">w Parameters</param>
        /// <param name="lParam">l Parameters</param>
        /// <returns>whether message was successfully sent.</returns>
        [DllImport("User32.dll")]
        internal static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam);

        /// <summary>
        /// The WinEventDelegate
        /// </summary>
        /// <param name="hWinEventHook">The hWinEventHook<see cref="IntPtr"/></param>
        /// <param name="eventType">The eventType<see cref="uint"/></param>
        /// <param name="hwnd">The hwnd<see cref="IntPtr"/></param>
        /// <param name="idObject">The idObject<see cref="int"/></param>
        /// <param name="idChild">The idChild<see cref="int"/></param>
        /// <param name="dwEventThread">The dwEventThread<see cref="uint"/></param>
        /// <param name="dwmsEventTime">The dwmsEventTime<see cref="uint"/></param>
        internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        /// <summary>
        /// The SetWinEventHook
        /// </summary>
        /// <param name="eventMin">The eventMin<see cref="uint"/></param>
        /// <param name="eventMax">The eventMax<see cref="uint"/></param>
        /// <param name="hmodWinEventProc">The hmodWinEventProc<see cref="IntPtr"/></param>
        /// <param name="lpfnWinEventProc">The lpfnWinEventProc<see cref="WinEventDelegate"/></param>
        /// <param name="idProcess">The idProcess<see cref="uint"/></param>
        /// <param name="idThread">The idThread<see cref="uint"/></param>
        /// <param name="dwFlags">The dwFlags<see cref="uint"/></param>
        /// <returns>The <see cref="IntPtr"/></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        /// <summary>
        /// The UnhookWinEvent
        /// </summary>
        /// <param name="hWinEventHook">The hWinEventHook<see cref="IntPtr"/></param>
        /// <returns>The <see cref="int"/></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
    }
}
