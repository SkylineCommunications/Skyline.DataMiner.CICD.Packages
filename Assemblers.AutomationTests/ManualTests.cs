namespace Assemblers.AutomationTests
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;

    [TestClass]
    [Ignore("This class is intended for manual tests for debugging.")]
    public class ManualTests
    {
        [TestMethod]
        public async Task AutomationScriptBuilder_Custom()
        {
            var path = "C:\\GitHub\\Skyline.DataMiner.Sdk\\SdkTests\\Test Files\\Package 6\\My Package\\My Package.csproj";

            Project project = Project.Load(path);

            var script = Script.Load(FileSystem.Instance.Path.Combine(project.ProjectDirectory, $"{project.ProjectName}.xml"));
            var scriptProjects = new Dictionary<string, Project>
            {
                // Will always be one
                [project.ProjectName] = project,
            };

            List<Script> allScripts = new List<Script>();

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(script, scriptProjects, allScripts, project.ProjectDirectory);

            var result = await builder.BuildAsync().ConfigureAwait(false);

            result.Should().NotBeNull();
        }
    }
}