namespace CenterTaskbar
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="Program" />
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Defines the customAppContext
        /// </summary>
        private static AppContext customAppContext;

        /// <summary>
        /// Defines the <see cref="MessageFilter" />
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private class MessageFilter : IMessageFilter
        {
            /// <summary>
            /// DisplayChange Event message code.
            /// </summary>
            private const int WM_DISPLAYCHANGE = 0x007e;

            /// <summary>
            /// Close Event message code.
            /// </summary>
            private const int WM_CLOSE = 0x0010;

            /// <inheritdoc />
            public bool PreFilterMessage(ref Message m)
            {
                if (customAppContext == null)
                {
                    return false;
                }
  
                switch (m.Msg)
                {
                    case WM_CLOSE:
                        Debug.Print("WM_CLOSE Event occurred");
                        customAppContext.Exit(null, null);
                        return true;

                    case WM_DISPLAYCHANGE:
                        Debug.Print("WM_DISPLAYCHANGE Event occurred");
                        customAppContext.Reload(null, null);
                        return false;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">startup arguments<see cref="string[]"/></param>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new MessageFilter());

            customAppContext = new AppContext(args);
            Application.Run(customAppContext);
        }
    }
}