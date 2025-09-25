namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Parsers.Protocol.Xml.QActions;

    /// <summary>
    /// Protocol assembler helper class.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Defines the assemblies that are referenced by default in DataMiner when a QAction is compiled.
        /// </summary>
        public static readonly IReadOnlyList<string> QActionDefaultImportDLLs = new List<string>()
        {
            "Interop.SLDms.dll",
            "mscorlib.dll",
            "QactionHelperBaseClasses.dll",
            "Skyline.DataMiner.Storage.Types.dll",
            "SLLoggerUtil.dll",
            "SLManagedScripting.dll",
            "SLNetTypes.dll",
            "System.dll",
            "System.Xml.dll",
            "System.Core.dll",
        };

        /// <summary>
        /// Retrieves the ID of the specified QAction project reference. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="reference">The project reference.</param>
        /// <param name="id">The ID of the specified QAction project reference</param>
        /// <returns><c>true</c> if the ID was successfully retrieved; otherwise, <c>false</c>.</returns>
        public static bool TryGetQActionId(ProjectReference reference, out int id)
        {
            var m = QAction.RegexExtractQActionID.Match(reference.Name);
            if (m.Success)
            {
                id = Convert.ToInt32(m.Groups["id"].Value);
                return true;
            }

            id = default(int);
            return false;
        }
    }
}
