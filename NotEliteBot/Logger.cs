using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotEliteBot
{
    class Debug
    {
        public static void Log(string message, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Action:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            string prefix = logLevel switch
            {
                LogLevel.Action => "",
                LogLevel.Info => " [INFO]",
                LogLevel.Warning => " [WARNING]",
                LogLevel.Error => " [ERROR]",
                _ => " [LOG]"
            };
            Console.WriteLine($"[{DateTime.Now}]{prefix} {message}");
            Console.ResetColor();
        }

        public enum LogLevel
        {
            Action,
            Info,
            Warning,
            Error
        }
    }
}
