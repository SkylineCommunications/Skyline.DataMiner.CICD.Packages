namespace Skyline.DataMiner.CICD.Packages.TestHelpers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Shared assembly initialization logic for all test projects.
    /// </summary>
    public static class AssemblyInitializer
    {
        /// <summary>
        /// Called by [AssemblyInitialize] in each test project's TestHelper.
        /// Registers MSBuild and creates the test fixture root directory.
        /// </summary>
        /// <param name="context">The test context.</param>
        public static void OnAssemblyInitialize(TestContext context)
        {
            TestFixture.EnsureMsBuildRegistered();
            FileSystem.Instance.Directory.CreateDirectory(TestFixture.TestFixtureRoot);
        }

        /// <summary>
        /// Called by [AssemblyCleanup] in each test project's TestHelper.
        /// Deletes the test fixture root directory.
        /// </summary>
        public static void OnAssemblyCleanup()
        {
            TestFixture.CleanupRoot();
        }
    }
}
