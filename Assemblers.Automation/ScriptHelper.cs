namespace Skyline.DataMiner.CICD.Assemblers.Automation
{
    using System;
    using System.Linq;

    /// <summary>
    /// Helper class for use with assembling a Visual Studio solution into a DataMiner file (e.g. protocol XML file or Automation script).
    /// </summary>
    public static class ScriptHelper
    {
        /// <summary>
        /// The DLLs that are referenced by default by DataMiner.
        /// </summary>
        public static readonly string[] ScriptDefaultImportDLLs =
        {
            "mscorlib.dll",
            "Skyline.DataMiner.Storage.Types.dll",
            "SLAnalyticsTypes.dll",
            "SLLoggerUtil.dll",
            "SLManagedAutomation.dll",
            "SLNetTypes.dll",
            "System.dll",
            "System.Core.dll",
            "System.Xml.dll",
        };

        /// <summary>
        /// The DLLs that need to be referenced with the Files path.
        /// </summary>
        public static readonly string[] DllsWithFilesPath =
        {
            "SLSRMLibrary.dll",
        };

        /// <summary>
        /// The DLLs that need to be referenced with the ProtocolScripts path.
        /// </summary>
        public static readonly string[] DllsWithProtocolScriptsPath =
        {
            "DataMinerSolutions.dll",
            "ProcessAutomation.dll"
        };

        /// <summary>
        /// Gets a value indicating whether the specified DLL is by default referenced by DataMiner.
        /// </summary>
        /// <param name="dll">The DLL.</param>
        /// <returns><c>true</c> if the DLL is by default referenced by DataMiner; otherwise, <c>false</c>.</returns>
        public static bool IsDefaultImportDll(string dll)
        {
            return ScriptDefaultImportDLLs.Contains(dll, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a value indicating whether the specified DLL needs to be referenced via the Files folder instead of the default ProtocolScripts.
        /// </summary>
        /// <param name="dll">Name of the DLL.</param>
        /// <returns><c>true</c> if the DLL needs to be referenced via the Files folder; otherwise <c>false</c>.</returns>
        public static bool NeedsFilesPath(string dll)
        {
            return DllsWithFilesPath.Contains(dll, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a value indicating whether the specified DLL needs to be referenced via the ProtocolScripts folder instead of the default ProtocolScripts\DllImport.
        /// </summary>
        /// <param name="dll">Name of the DLL.</param>
        /// <returns><c>true</c> if the DLL needs to be referenced via the ProtocolScripts folder; otherwise <c>false</c>.</returns>
        public static bool NeedsProtocolScriptsPath(string dll)
        {
            return DllsWithProtocolScriptsPath.Contains(dll, StringComparer.OrdinalIgnoreCase);
        }
    }
}
