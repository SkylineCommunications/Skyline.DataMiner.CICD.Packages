namespace Skyline.DataMiner.CICD.DMApp.Common
{
	using System.Diagnostics;

	/// <summary>
	/// Allows running dotnet commands.
	/// </summary>
	public interface IDotnet
	{
		bool Run(string command, bool ignoreOutput);

		(bool succes, string output, string errors) Run(string command);

		bool Run(string command, DataReceivedEventHandler overrideOnOutput, DataReceivedEventHandler overrideOnError);
	}
}