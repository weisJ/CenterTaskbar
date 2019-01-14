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
        /// <param name="hWndInsertAfter">Handle to the window to precede the positioned window in the z-order.
        /// This parameter must be a window handle or one of the following values.<see cref="IntPtr"/></param>
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

    }
}