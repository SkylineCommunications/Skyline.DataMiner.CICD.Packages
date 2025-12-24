namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Common.NuGet;

    internal static class NuGetHelper
    {
        public static readonly IReadOnlyDictionary<string, (string path, bool inDllImportDirectory)> CustomNuGetPackages = new Dictionary<string, (string path, bool inDllImportDirectory)>
        {
            ["Skyline.DataMiner.Core.SRM"] = (@"SRM\SLSRMLibrary.dll", true),
            ["Skyline.DataMiner.Core.SRM.Utils.Dijkstra"] = (@"SRM\SLDijkstraSearch.dll", true),
            ["Skyline.DataMiner.Core.SRM.Utils.IAS"] = (@"SRM\Skyline.DataMiner.Core.SRM.Utils.IAS.dll", true)
        };
        
        public static bool IsDevPackNuGetPackage(string packageId)
        {
            return DevPackHelper.DevPackNuGetPackages.Contains(packageId) || packageId.StartsWith(DevPackHelper.FilesPrefix);
        }

        public static bool SkipPackageDependencies(string packageId)
        {
            return CustomNuGetPackages.ContainsKey(packageId) || IsSolutionLibraryNuGetPackage(packageId, out _);
        }

        public static bool IsSolutionLibraryNuGetPackage(string packageId, out string name)
        {
            // Solution 'devpacks' (like MediaOps)
            const string prefix = "Skyline.DataMiner.Dev.Utils.";

            if (packageId.StartsWith(prefix))
            {
                name = packageId.Substring(prefix.Length);
                return true;
            }

            name = null;
            return false;
        }
    }
}