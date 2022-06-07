using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor
{
    public class Log
    {
        
    }
    public static class LogHelper
    {
        public static void Info(string type, string message, bool status = true)
        {
            Console.WriteLine($"{(status?"[+]":"[-]")}[{DateTime.Now:G}][{type}]{message}");
        }
    }
}
