using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success,
        Failure
    }
    public class LogMessage
    {
        public LogLevel Level { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}
