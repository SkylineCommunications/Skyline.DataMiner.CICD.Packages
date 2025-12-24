namespace Parsers.ProtocolTests
{
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.FileSystem;

    [TestClass]
	public class AssemblyInitialize
	{
		[AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // This is needed because certain tools will look at all csproj files in the entire repository.

	        var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var zipFile in FileSystem.Instance.Directory.GetFiles(baseDir, "*.zip", SearchOption.AllDirectories))
	        {
		        string dir = FileSystem.Instance.Path.Combine(FileSystem.Instance.Path.GetDirectoryName(zipFile), "TestFiles");
                
		        ZipFile.ExtractToDirectory(zipFile, dir);
	        }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
	        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // TODO: Add GetDirectories to FileSystem
            foreach (var testFilesDirectories in Directory.GetDirectories(baseDir, "TestFiles", SearchOption.AllDirectories))
	        {
                // TODO: Add Delete to FileSystem (with recursive option)
                Directory.Delete(testFilesDirectories, true);
	        }
        }
    }
}