using System;
using System.IO;
using System.Text;

namespace Mekitamete
{
    internal static class Logger
    {
        private static readonly object loggerLock = new object();
        private static FileStream loggerFile = null;
        internal enum MessageLevel
        {
            Informational,
            Warning,
            Error
        }

        private static void WriteToConsoleAndFile(string msg)
        {
            Console.Write(msg);

            if (loggerFile != null)
            {
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
                loggerFile.Write(msgBuffer, 0, msgBuffer.Length);
            }
        }

        /// <summary>
        /// Opens or closes a log file.
        /// </summary>
        /// <param name="fileName">Name/path of the log file, or null to close the log file.</param>
        internal static void OpenLogFile(string fileName)
        {
            if (loggerFile != null)
            {
                loggerFile.Close();
                loggerFile = null;
            }

            if (fileName != null)
            {
                loggerFile = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
        }

        /// <summary>
        /// Prints a debug message.
        /// </summary>
        /// <param name="section">The application component that the message is related to.</param>
        /// <param name="msg">Message to print.</param>
        /// <param name="level">The message severity: various levels produce different console colors and line prefixes.</param>
        internal static void Log(string section, string msg, MessageLevel level = MessageLevel.Informational)
        {
            lock (loggerLock)
            {
                Console.ForegroundColor = (level == MessageLevel.Error) ? ConsoleColor.Red : (level == MessageLevel.Warning ? ConsoleColor.Yellow : ConsoleColor.Cyan);
                WriteToConsoleAndFile(string.Format("[{1} {0}]", DateTime.Now.ToString("yyyy-MM-dd HH\\:mm\\:ss"), (level == MessageLevel.Error) ? "E" : (level == MessageLevel.Warning ? "W" : "I")));
                Console.ForegroundColor = ConsoleColor.White;
                WriteToConsoleAndFile($" - {section}: {msg}\n");

                if (loggerFile != null)
                {
                    loggerFile.Flush();
                }
            }
        }
    }
}
