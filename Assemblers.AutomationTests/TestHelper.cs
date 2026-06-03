namespace Assemblers.AutomationTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Packages.TestHelpers;

    [TestClass]
    public static class TestHelper
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            AssemblyInitializer.OnAssemblyInitialize(context);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            AssemblyInitializer.OnAssemblyCleanup();
        }
    }
}