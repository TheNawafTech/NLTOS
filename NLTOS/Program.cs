using NLTOS.Applications;
using NLTOS.Classes;
using NLTOS.Login;
using NLTOS.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security;
namespace NLTOS
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EnsureEventLogSource();

            Application.Run(new frmLogin());
        }

        /// <summary>
        /// Registers the application's event log source if it is not registered yet.
        /// Registering a source requires administrator rights and only has to happen
        /// once per machine, so a standard user is expected to fail here. That is not
        /// a reason to stop: the application runs, it just cannot write event log
        /// entries. Any other failure is left to surface.
        /// </summary>
        private static void EnsureEventLogSource()
        {
            try
            {
                if (!EventLog.SourceExists(clsGlobal.SourceName))
                    EventLog.CreateEventSource(clsGlobal.SourceName, "Application");
            }
            catch (SecurityException)
            {
                // Not running elevated, so the source cannot be inspected or created.
            }
            catch (UnauthorizedAccessException)
            {
                // The same condition, reported differently on some Windows versions.
            }
        }
    }
}
