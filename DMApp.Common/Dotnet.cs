namespace Skyline.DataMiner.CICD.DMApp.Common
{
	using System.Diagnostics;
	using System.Text;

	internal class Dotnet : IDotnet
	{
		private readonly DataReceivedEventHandler onOutput;
		private readonly DataReceivedEventHandler onError;

		public Dotnet(DataReceivedEventHandler onOutput, DataReceivedEventHandler onError)
		{
			this.onOutput = onOutput;
			this.onError = onError;
		}

		public bool Run(string command, bool ignoreOutput)
		{
			bool success = false;

			if (!ignoreOutput)
			{
				success = Run(command).succes;
			}
			else
			{
				string pathTo = "dotnet";
				ProcessStartInfo details = new ProcessStartInfo(pathTo, command);

				details.CreateNoWindow = true;
				details.UseShellExecute = false;

				using (var process = new Process())
				{
					process.StartInfo = details;

					if (process.Start())
					{
						process.WaitForExit();
						success = process.ExitCode == 0;
					}
					else
					{
						success = false;
					}
				}
			}

			return success;
		}

		public (bool succes, string output, string errors) Run(string command)
		{
			StringBuilder totalOutput = new StringBuilder();
			StringBuilder totalErrors = new StringBuilder();

			DataReceivedEventHandler onOutputWrapped = (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					totalOutput.AppendLine(e.Data);
					onOutput(sender, e);
				}
			};

			DataReceivedEventHandler onErrorWrapped = (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					totalErrors.AppendLine(e.Data);
					onError(sender, e);
				}
			};

			bool success = Run(command, onOutputWrapped, onErrorWrapped);

			return (success, totalOutput.ToString(), totalErrors.ToString());
		}

		public bool Run(string command, DataReceivedEventHandler overrideOnOutput, DataReceivedEventHandler overrideOnError)
		{
			string pathTo = "dotnet";
			ProcessStartInfo details = new ProcessStartInfo(pathTo, command);

			details.CreateNoWindow = true;
			details.UseShellExecute = false;
			details.RedirectStandardError = true;
			details.RedirectStandardOutput = true;
		
			bool success = false;


			using (var process = new Process())
			{
				process.StartInfo = details;

				process.OutputDataReceived += overrideOnOutput;
				process.ErrorDataReceived += overrideOnError;

				if (process.Start())
				{
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					process.WaitForExit();
					success = process.ExitCode == 0;
				}
				else
				{
					success = false;
				}
			}

			return success;
		}
	}
}