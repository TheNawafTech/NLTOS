using System;
using NLTOS_DataAccess;

namespace NLTOS_Business
{
    /// <summary>
    /// Lets the presentation layer record diagnostics without referencing the data
    /// access layer directly, which would point a layer boundary the wrong way.
    ///
    /// The work belongs to clsEventLogger, which disables itself rather than throw
    /// when the event log is unavailable, so calling this is always safe.
    /// </summary>
    public static class clsLogger
    {
        public static void LogError(string operation, Exception ex)
        {
            clsEventLogger.LogError(operation, ex);
        }
    }
}
