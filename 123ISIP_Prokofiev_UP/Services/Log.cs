using System;
using System.IO;

namespace _123ISIP_Prokofiev_UP.Services
{

    public static class Log
    {
        private static readonly string Path =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");

        public static void Write(string message)
        {
            try { File.AppendAllText(Path, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message + Environment.NewLine); }
            catch {  }
        }
    }
}
