namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Skyline.DataMiner.Automation;

    public class OrderLogger : IDisposable
    {
        private readonly IEngine engine;

        private readonly StringBuilder automationLoggerBuffer = new StringBuilder();

        /// <summary>
        /// Initialize a new OrderLogger object.
        /// This constructor should only be used in case the order has not been booked.
        /// Make sure as soon as the order has been booked that AddOrderReference is executed to make sure the logging is correctly generated.
        /// </summary>
        public OrderLogger(IEngine engine, params string[] fileNames)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this.FileNames = fileNames?.ToList() ?? new List<string>();
        }

        public string DefaultFilePath { get; set; }

        public List<string> FileNames { get; set; } = new List<string>();

        public void Log(string nameOfClass, string nameOfMethod, string message, string nameOfObject = null)
        {
            automationLoggerBuffer.AppendLine($"[{DateTime.Now.ToString("o")}] {nameOfClass}|{nameOfMethod}{(nameOfObject != null ? $"|{nameOfObject}" : string.Empty)}|{message}");

            if (automationLoggerBuffer.Length > 10000000L)
            {
                Dispose();
                automationLoggerBuffer.Clear();
            }
        }

        public void Dispose()
        {
            if (FileNames.Any())
            {
                foreach (var fileName in FileNames.Distinct())
                {
                    string path = FixedFileLogger.GenerateLogFilePath(fileName);
                    FixedFileLogger fixedFileLogger = new FixedFileLogger(engine, path);
                    fixedFileLogger.Log(automationLoggerBuffer.ToString());
                    fixedFileLogger.Dispose();
                }
            }
            else if (!string.IsNullOrWhiteSpace(DefaultFilePath))
            {
                string path = FixedFileLogger.GenerateLogFilePath(DefaultFilePath);
                FixedFileLogger fixedFileLogger = new FixedFileLogger(engine, path);
                fixedFileLogger.Log(automationLoggerBuffer.ToString());
                fixedFileLogger.Dispose();
            }
            else
            {
                engine.Log(automationLoggerBuffer.ToString());
            }
        }
    }
}
