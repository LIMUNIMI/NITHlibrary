namespace NITHlibrary.Tools.Logging
{
    /// <summary>
    /// Provides logging services for exceptions to a log file.
    /// Log files will be stored in a dedicated directory, inside the application's base directory.
    /// File names will be formatted as follows, to include the currend date: "Errors_yyyyMMdd.log".
    /// </summary>
    public static class LoggingService
    {
        /// <summary>
        /// The directory where log files will be stored.
        /// </summary>
        public static string LogFileDirectory = "Logs";

        /// <summary>
        /// Static constructor to initialize the logging directory.
        /// </summary>
        static LoggingService()
        {
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileDirectory);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        /// <summary>
        /// Logs the provided exception to a log file, including inner exceptions if present.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="isInnerException">Indicates if the logged exception is an inner exception.</param>
        public static void Log(Exception exception, bool isInnerException = false)
        {
            while (true)
            {
                using (StreamWriter sw = new(LogFileName(), true))
                {
                    sw.WriteLine(isInnerException ? "INNER EXCEPTION" : $"EXCEPTION: {DateTime.Now}");
                    sw.WriteLine(new string(isInnerException ? '-' : '=', 40));
                    sw.WriteLine($"{exception.Message}");
                    sw.WriteLine($"{exception.StackTrace}");
                    sw.WriteLine(); // Blank line, to make the log file easier to read
                }

                if (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    isInnerException = true;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Generates the log file name based on the current date.
        /// </summary>
        /// <returns>The log file name for today's date.</returns>
        private static string LogFileName()
        {
            // This will create a separate log file for each day.
            // Not that we're hoping to have many days of errors.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileDirectory,
                                $"Errors_{DateTime.Now:yyyyMMdd}.log");
        }
    }
}