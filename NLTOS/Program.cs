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
            // Application.Run(new frmMain());
            // Application.Run(new frmTest2());

            if (!EventLog.SourceExists(clsGlobal.SourceName))
            {
                EventLog.CreateEventSource(clsGlobal.SourceName, "Application");
                Console.WriteLine($"Event source '{clsGlobal.SourceName}' created successfully.");
            }

            Application.Run(new frmLogin());



        }
    }
}
