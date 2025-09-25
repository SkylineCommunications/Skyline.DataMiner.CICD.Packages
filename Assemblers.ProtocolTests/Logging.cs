namespace Assemblers.ProtocolTests
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

        public void ReportError(string error)
        {
            ReportLog($"ERROR|{error}");
        }

        public void ReportStatus(string status)
        {
            ReportLog($"STATUS|{status}");
        }

        public void ReportWarning(string warning)
        {
            ReportLog($"WARNING|{warning}");
        }

        public void ReportDebug(string debug)
        {
            ReportLog($"DEBUG|{debug}");
        }

        public void ReportLog(string message)
        {
            if (debug)
            {
                Console.WriteLine(message);
            }
        }
    }
}
