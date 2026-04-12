using System.Diagnostics;
using System.Security;

namespace Intalio.Tools.Common.Logging
{
    public class LogToEventViewer
    {
        #region Private static fields

        private const string SOURCE_NAME = "MMS";
        private const string LOG_NAME = "Application";
        private static EventLog? _current;
        private static bool _initialized;
        private static bool _isAvailable = true;
        private static readonly object _lock = new();

        #endregion

        #region Private static Properties

        private static EventLog? Current
        {
            get
            {
                if (!_isAvailable)
                    return null;

                if (!_initialized)
                {
                    lock (_lock)
                    {
                        if (!_initialized)
                        {
                            _initialized = true;
                            try
                            {
                                // Try to check if source exists - this may fail without admin rights
                                bool sourceExists = false;
                                try
                                {
                                    sourceExists = EventLog.SourceExists(SOURCE_NAME);
                                }
                                catch (SecurityException)
                                {
                                    // Cannot check - assume it exists and try to use it
                                    sourceExists = true;
                                }

                                if (!sourceExists)
                                {
                                    // Creating event source requires admin privileges
                                    // This should be done during installation, not at runtime
                                    try
                                    {
                                        EventLog.CreateEventSource(SOURCE_NAME, LOG_NAME);
                                    }
                                    catch (SecurityException)
                                    {
                                        // Cannot create source - fall back to Application source
                                        _current = new EventLog(LOG_NAME);
                                        _current.Source = "Application";
                                        return _current;
                                    }
                                }

                                _current = new EventLog(LOG_NAME);
                                _current.Source = SOURCE_NAME;
                            }
                            catch (Exception)
                            {
                                // Event logging not available - disable silently
                                _isAvailable = false;
                                _current = null;
                            }
                        }
                    }
                }
                return _current;
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Log information to event viewer
        /// </summary>
        /// <param name="message"></param>
        public static void LogMessage(string message)
        {
            try
            {
                Current?.WriteEntry(message, EventLogEntryType.Information);
            }
            catch
            {
                // Silently fail - logging should not break the application
            }
        }

        /// <summary>
        /// Log exception to event viewer
        /// </summary>
        /// <param name="ex"></param>
        public static void LogException(Exception ex)
        {
            try
            {
                string msg = string.Format("Source: {0} \nMessage: {1} \nStackTrace: {2} \nInnerException: {3}"
                    , ex.Source
                    , ex.Message
                    , ex.StackTrace
                    , ex.InnerException != null ? ex.InnerException.Message : "");
                Current?.WriteEntry(msg, EventLogEntryType.Error);
            }
            catch
            {
                // Silently fail - logging should not break the application
            }
        }

        public static void LogException(Exception ex, string additionalMessage)
        {
            try
            {
                string msg = string.Format("additionalMessage: {0} \nSource: {1} \nMessage: {2} \nStackTrace: {3} \nInnerException: {4}"
                    , additionalMessage
                    , ex.Source
                    , ex.Message
                    , ex.StackTrace
                    , ex.InnerException != null ? ex.InnerException.Message : "");
                Current?.WriteEntry(msg, EventLogEntryType.Error);
            }
            catch
            {
                // Silently fail - logging should not break the application
            }
        }

        #endregion
    }
}
