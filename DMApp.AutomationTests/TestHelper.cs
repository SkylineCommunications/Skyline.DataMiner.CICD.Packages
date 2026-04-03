namespace DMApp.AutomationTests
{
    using System.Runtime.InteropServices;

    using Microsoft.Build.Locator;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public static class TestHelper
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Register MSBuild so it can find the .NET SDK
            if (!MSBuildLocator.IsRegistered && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
    }
}