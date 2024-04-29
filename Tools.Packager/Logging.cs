namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System;
    using Skyline.DataMiner.CICD.Loggers;

    internal class Logging : ILogCollector
    {
        private readonly bool debug;

        public Logging(bool debug)
        {
            this.debug = debug;
        }

        public void ReportDebug(string debug)
        {
            ReportLog($"DEBUG|{debug}");
        }

        public void ReportError(string error)
        {
            ReportLog($"ERROR|{error}");
        }

        public void ReportLog(string message)
        {
            if (debug)
            {
                Console.WriteLine(message);
            }
        }

        public void ReportStatus(string status)
        {
            ReportLog($"STATUS|{status}");
        }

        public void ReportWarning(string warning)
        {
            ReportLog($"WARNING|{warning}");
        }
    }
}