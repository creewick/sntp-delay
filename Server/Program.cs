using System;
using System.Collections.Generic;

namespace Server
{
    public static class Program
    {
        private static int[] DefaultSettings => new[] { 123, 1024, Environment.ProcessorCount };
        
        private static int[] GetSettings(string[] args)
        {
            var settings = DefaultSettings;
            for (var i = 0; i < args.Length; i++)
                if (int.TryParse(args[i], out var value))
                    settings[i] = value;
            return settings;
        }
        
        public static void Main(string[] args)
        {
            var server = new SntpServer(GetSettings(args));
            server.Start();
        }
    }
}