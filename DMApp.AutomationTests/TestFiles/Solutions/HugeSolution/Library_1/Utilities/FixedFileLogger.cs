namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using NPOI.SS.UserModel;
	using Skyline.DataMiner.Automation;

	public class FixedFileLogger : IDisposable
	{
		private const long maxLogFileSize = 100000000; // 100 mb
		public static readonly string SkylineDataFilePath = @"C:\Skyline_Data\";
		public static readonly string TextFileExtension = ".txt";

		private readonly IEngine engine;

		private readonly List<string> buffer = new List<string>();

		public FixedFileLogger(IEngine engine, params string[] logFilePaths)
		{
			this.engine = engine;
			LogfilePaths = logFilePaths?.ToList() ?? throw new ArgumentNullException(nameof(logFilePaths));

			foreach (var logFilePath in logFilePaths)
			{
				if (!logFilePath.StartsWith(SkylineDataFilePath)) throw new ArgumentException($"Argument does not start with {SkylineDataFilePath}", nameof(logFilePaths));
				if (!logFilePath.EndsWith(TextFileExtension)) throw new ArgumentException($"Argument does not end with {TextFileExtension}", nameof(logFilePaths));
			}

			TryCreateNewFiles();
		}

		public List<string> LogfilePaths { get; private set; }

		public void Log(string message)
		{
			buffer.Add(message);
		}

		public void Log(string nameOfClass, string nameOfMethod, string message, string nameOfObject = null)
		{
			buffer.Add($"[{DateTime.Now.ToString("o")}] {nameOfClass}|{nameOfMethod}{(nameOfObject != null ? $"|{nameOfObject}" : string.Empty)}|{message}");
		}

		public static string GenerateLogFilePath(string fileName)
		{
			return $"{SkylineDataFilePath}{fileName}{TextFileExtension}";
		}

		public void Dispose()
		{
			try
			{
				TryCreateNewFiles();

				long bufferSize = buffer.Sum(x => Encoding.ASCII.GetBytes(x).Length);

				foreach (var logfilePath in LogfilePaths)
				{
					long currentFileSize = new FileInfo(logfilePath).Length; // 0 in case file doesn't exist

					long potentialFileSize = currentFileSize + bufferSize;

					if (potentialFileSize > maxLogFileSize)
					{
						long amountOfBytesOverTheMax = currentFileSize - maxLogFileSize;
						long extraAmountOfBytesToRemoveToHaveSomeMargin = maxLogFileSize / 4;
						long totalAmountOfBytesToSkip = amountOfBytesOverTheMax + extraAmountOfBytesToRemoveToHaveSomeMargin;

						var linesToKeep = GetFileLines(logfilePath, totalAmountOfBytesToSkip).ToList();
						linesToKeep.AddRange(buffer);

						WriteLinesWithRetry(logfilePath, linesToKeep);
					}
					else
					{
						AppendLinesWithRetry(logfilePath, buffer);
					}
				}

				buffer.Clear();
			}
			catch (Exception e)
			{
				engine.Log(nameof(FixedFileLogger), nameof(Dispose), $"Something went wrong for logfiles {string.Join(", ", LogfilePaths)}: {e}");
				throw;
			}
		}

		private static void WriteLinesWithRetry(string logfilePath, List<string> linesToKeep)
		{
			int retry = 0;
			bool successful = false;
			while (retry < 10 && !successful)
			{
				try
				{
					File.WriteAllLines(logfilePath, linesToKeep);
					successful = true;
				}
				catch (IOException)
				{
					successful = false;
					retry++;
					Thread.Sleep(50);
				}
			}
		}

		private void AppendLinesWithRetry(string logfilePath, List<string> linesToAppend)
		{
			int retry = 0;
			bool successful = false;
			while (retry < 10 && !successful)
			{
				try
				{
					File.AppendAllLines(logfilePath, linesToAppend);
					successful = true;
				}
				catch (IOException)
				{
					successful = false;
					retry++;
					Thread.Sleep(50);
				}
			}
		}

		private static IEnumerable<string> GetFileLines(string filename, long amountOfBytesToSkip = 0)
		{
			// method to avoid having too much lines in memory 
			// https://stackoverflow.com/questions/7008542/removing-the-first-line-of-a-text-file-in-c-sharp

			using (var stream = File.OpenRead(filename))
			{
				using (var reader = new StreamReader(stream))
				{
					long sumOfBytes = 0;
					string line;

					while ((line = reader.ReadLine()) != null)
					{
						sumOfBytes += Encoding.ASCII.GetBytes(line).Length;
						if (sumOfBytes <= amountOfBytesToSkip) continue;

						yield return line;
					}
				}
			}
		}

		private void TryCreateNewFiles()
		{
			foreach (var logfilePath in LogfilePaths)
			{
				int retries = 0;
				bool successful = false;
				Exception exception = null;
				while (!successful && retries < 5)
				{
					try
					{
						bool logFileExists = File.Exists(logfilePath);

						if (!logFileExists)
						{
							var newFile = File.Create(logfilePath);
							newFile.Close();
						}

						successful = true;
					}
					catch (Exception e)
					{
						successful = false;
						retries++;
						exception = e;

						Thread.Sleep(1000);
					}
				}

				if (!successful)
				{
					Log(nameof(FixedFileLogger), nameof(TryCreateNewFiles), $"Something went wrong for logfile {logfilePath}: {exception}");
					throw exception;
				}
			}
		}
	}
}
