namespace Assemblers.CommonTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;

    [TestClass]
    [Ignore("This class is intended for manual tests for debugging.")]
    public class ManualTests
    {
        [TestMethod]
        public void Project_Load_Custom()
        {
            string projectFile = @"C:\GitHub\SLC-S-MediaOps\SLC-S-MediaOps\SLC-S-MediaOps.csproj";

            var project = Project.Load(projectFile);
            
            Assert.AreEqual(projectFile, project.Path);
        }
    }
}