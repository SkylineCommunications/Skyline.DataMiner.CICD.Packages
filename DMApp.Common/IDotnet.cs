namespace Skyline.DataMiner.CICD.DMApp.Common
{
    /// <summary>
    /// Allows running dotnet commands.
    /// </summary>
    public interface IDotnet
    {
        /// <summary>
        /// Will run a specific dotnet command.
        /// </summary>
        /// <param name="command">The dotnet command.</param>
        /// <param name="output">Any output from running the command.</param>
        /// <param name="errors">Any error from the command.</param>
        /// <returns><see cref="Boolean.TrueString"/> if there were no errors with the command.</returns>
        bool Run(string command, out string output, out string errors);
    }
}