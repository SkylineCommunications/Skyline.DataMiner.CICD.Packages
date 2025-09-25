namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    internal static class DevPackHelper
    {
        private const string CommonDevPackName = "Skyline.DataMiner.Dev.Common";

        private static readonly HashSet<string> DevPackDllsToIgnore = new HashSet<string>
        {
            "ICSharpCode.SharpZipLib.dll",
            "Newtonsoft.Json.dll",
            "protobuf-net.dll",
            "SLProtoBufLibrary.dll",
        };

        public static bool IsDevPackDllReference(Reference reference)
        {
            string referenceDllName = reference.GetDllName();

            return DevPackDllsToIgnore.Contains(referenceDllName) &&
                   (reference.HintPath == null || reference.HintPath.Contains(CommonDevPackName));
        }
    }
}