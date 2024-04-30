namespace Skyline.DataMiner.CICD.DMApp.Common
{
    /// <summary>
    /// Allows running dotnet commands.
    /// </summary>
    public interface IDotnet
	{
        /// <summary>
        /// Run a dotnet command.
        /// </summary>
        /// <param name="command">Command to run.</param>
        /// <param name="ignoreOutput"><c>True</c> to ignore the output.</param>
        /// <returns><c>True</c> when the command was successful.</returns>
		bool Run(string command, bool ignoreOutput);

        /// <summary>
        /// Run a dotnet command.
        /// </summary>
        /// <param name="command">Command to run.</param>
        /// <returns><c>True</c> when the command was successful and the corresponding output and errors.</returns>
		(bool succes, string output, string errors) Run(string command);
	}
}