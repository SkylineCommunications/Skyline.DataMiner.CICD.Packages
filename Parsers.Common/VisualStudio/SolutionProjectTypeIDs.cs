namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;

    /// <summary>
    /// Defines the GUID values for the different items in a Visual Studio solution.
    /// </summary>
    public static class SolutionProjectTypeIDs
    {
        /// <summary>
        /// Visual Studio solution folder GUID.
        /// </summary>
        public static readonly Guid SolutionFolder = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        /// <summary>
        /// MSBuild project GUID.
        /// </summary>
        public static readonly Guid MsBuildProject = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

        /// <summary>
        /// .NET Core project GUID.
        /// </summary>
        public static readonly Guid NetCoreProject = new Guid("9A19103F-16F7-4668-BE54-9A1E7A4F7556");
    }
}
