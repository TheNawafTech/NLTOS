using System;
using System.Diagnostics;
using System.Security;

namespace NLTOS_DataAccess
{
    /// <summary>
    /// Writes diagnostics to the Windows event log.
    ///
    /// Logging is a side effect of handling an error, so it must never become an error
    /// of its own. Every failure to log is swallowed here, deliberately and in one
    /// place, instead of escaping from inside a catch block and replacing the original
    /// problem with a crash.
    ///
    /// Registering an event source requires administrator rights. When the source is
    /// not registered, writing to it would try to create it and fail, so logging is
    /// disabled for the session rather than attempted on every call.
    /// </summary>
    public static class clsEventLogger
    {
        private const string SourceName = "NLTOS";

        private static readonly object _gate = new object();

        private static bool _isAvailable;
        private static bool _isChecked;

        /// <summary>
        /// Records that <paramref name="operation"/> failed with <paramref name="ex"/>.
        /// Never throws.
        /// </summary>
        public static void LogError(string operation, Exception ex)
        {
            // The type is the first thing worth knowing when reading these entries back:
            // a SqlException and a NullReferenceException call for completely different
            // investigations, and the message alone does not always make clear which it was.
            string details = ex == null
                ? "No exception details available."
                : ex.GetType().FullName + ": " + ex.Message + "\n" + ex.StackTrace;

            Write($"[{DateTime.Now}] ERROR in {operation}\n{details}");
        }

        private static void Write(string message)
        {
            if (!IsAvailable())
                return;

            try
            {
                EventLog.WriteEntry(SourceName, message, EventLogEntryType.Error);
            }
            catch (Exception)
            {
                // The caller is already handling a failure; reporting it must not add a
                // second one, so nothing is rethrown. The event log is clearly not usable
                // in this session, so stop attempting it.
                _isAvailable = false;
            }
        }

        /// <summary>
        /// Reports whether the event source is registered. The check itself needs rights
        /// a standard user does not have, so it runs once and the answer is reused.
        /// </summary>
        private static bool IsAvailable()
        {
            if (_isChecked)
                return _isAvailable;

            lock (_gate)
            {
                if (_isChecked)
                    return _isAvailable;

                try
                {
                    _isAvailable = EventLog.SourceExists(SourceName);
                }
                catch (SecurityException)
                {
                    // Not running elevated: the source list cannot be inspected.
                    _isAvailable = false;
                }
                catch (UnauthorizedAccessException)
                {
                    // The same condition, reported differently on some Windows versions.
                    _isAvailable = false;
                }

                _isChecked = true;
            }

            return _isAvailable;
        }
    }
}
