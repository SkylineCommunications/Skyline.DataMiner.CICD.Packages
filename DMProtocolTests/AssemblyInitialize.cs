namespace DMProtocolTests
{
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssemblyInitialize
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // This is needed because certain tools will look at all csproj files in the entire repository.

            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var zipFile in Directory.GetFiles(baseDir, "*.zip", SearchOption.AllDirectories))
            {
                string dir = Path.Combine(Path.GetDirectoryName(zipFile), "TestFiles");

                ZipFile.ExtractToDirectory(zipFile, dir);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var testFilesDirectories in Directory.GetDirectories(baseDir, "TestFiles", SearchOption.AllDirectories))
            {
                Directory.Delete(testFilesDirectories, true);
            }
        }
    }
}