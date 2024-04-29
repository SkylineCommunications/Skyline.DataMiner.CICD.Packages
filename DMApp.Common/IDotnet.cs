namespace Skyline.DataMiner.CICD.DMApp.Common
{
    /// <summary>
    /// Allows running dotnet commands.
    /// </summary>
    public interface IDotnet
	{
		bool Run(string command, bool ignoreOutput);

		(bool succes, string output, string errors) Run(string command);
	}
}