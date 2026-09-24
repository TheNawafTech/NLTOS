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
using System.Threading;
using System.Data.SqlClient;
using NLTOS_Buisness;
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

            // Send exceptions that escape a UI event handler to OnUiThreadException
            // instead of the default WinForms error dialog, which offers to continue
            // and shows the stack trace.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUiThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            EnsureEventLogSource();

            Application.Run(new frmLogin());
        }

        /// <summary>
        /// Handles an exception that escaped a UI event handler.
        ///
        /// WinForms catches it before it reaches the message loop, so the loop keeps
        /// running and the window stays open once this returns. The action that failed
        /// did not complete, so the message tells the user exactly that rather than
        /// implying the application recovered.
        ///
        /// The details go to the event log; the user sees none of them, since an error
        /// message is not the place to print connection details or a stack trace.
        /// </summary>
        private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
        {
            clsLogger.LogError("Unhandled UI thread exception", e.Exception);

            // A failure reported by the database is something the application can expect
            // to meet, so it is named as such. It covers a wide range of causes - a lost
            // connection, a timeout, a permission or constraint problem, a deadlock - so
            // the message says only that the operation failed, and does not claim the
            // database is unreachable or that nothing was written.
            bool isDatabaseFailure = e.Exception is SqlException;

            string message = isDatabaseFailure
                ? "The database operation could not be completed." +
                  Environment.NewLine + Environment.NewLine +
                  "Please try again. If the problem continues, restart the application."
                : "An unexpected error occurred and the last action was not completed." +
                  Environment.NewLine + Environment.NewLine +
                  "Please try again. If it keeps happening, restart the application.";

            MessageBox.Show(
                message,
                isDatabaseFailure ? "Database Operation Failed" : "Unexpected Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// Last chance to record an exception nothing else handled.
        ///
        /// By the time this runs the runtime is already tearing the process down, so it
        /// only writes to the log. It cannot keep the application alive and does not
        /// pretend to, and it shows no dialog: the process may not survive long enough
        /// for anyone to read one.
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            clsLogger.LogError("Unhandled exception", e.ExceptionObject as Exception);
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
