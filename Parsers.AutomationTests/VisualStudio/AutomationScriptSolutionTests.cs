namespace Parsers.AutomationTests.VisualStudio
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Automation.VisualStudio;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;

    [TestClass]
    public class AutomationScriptSolutionTests
    {
        private static readonly string[] DllImportNewtonsoft = { "Newtonsoft.Json.dll" };

        #region Solution 1 (Basic)

        [TestMethod]
        public void AutomationScriptCompiler_Solution1_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.HasCount(3, solution.Projects);
            Assert.HasCount(2, solution.Scripts);

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.HasCount(2, script1.ScriptExes);
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(DllImportNewtonsoft, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.HasCount(1, script2.ScriptExes);
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 2 (Scripts in subfolders)

        [TestMethod]
        public void AutomationScriptCompiler_Solution2_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution2"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.HasCount(3, solution.Projects);
            Assert.HasCount(2, solution.Scripts);

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.HasCount(2, script1.ScriptExes);
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(DllImportNewtonsoft, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.HasCount(1, script2.ScriptExes);
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 3 (SharedProject)

        [TestMethod]
        public void AutomationScriptCompiler_Solution3_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution3"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.HasCount(3, solution.Projects);
            Assert.HasCount(2, solution.Scripts);

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.HasCount(2, script1.ScriptExes);
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(DllImportNewtonsoft, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.HasCount(1, script2.ScriptExes);
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 4 (Exception)

        [TestMethod]
        public void AutomationScriptCompiler_Solution4_Load()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution4"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            // Act
            Action act = () => AutomationScriptSolution.Load(path);

            // Assert
            act.Should().ThrowExactly<ParserException>().WithMessage("Could not find 'Scripts' folder in root of solution.");
        }

        #endregion

        [TestMethod]
        public void AutomationScriptCompiler_Solution5_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution5"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.HasCount(1, solution.Projects);
            Assert.HasCount(1, solution.Scripts);

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "New").Script;
            Assert.IsNotNull(script1);
            Assert.HasCount(1, script1.ScriptExes);
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:New_1]", exes1[0].Code);
            //CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll" }, exes1[0].DllImports.ToArray());
        }

        [TestMethod]
        public void AutomationScript_Solution6_LegacySln()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution6"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            VerifySolution6(solution, dir, path, false);
        }

        [TestMethod]
        public void AutomationScript_Solution6_Slnx()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solution6"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.slnx");

            var solution = AutomationScriptSolution.Load(path);

            VerifySolution6(solution, dir, path, true);
        }

        private void VerifySolution6(AutomationScriptSolution solution, string dir, string path, bool isSlnx)
        {
            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.HasCount(7, solution.Folders);
            Assert.HasCount(1, solution.Projects);
            Assert.HasCount(1, solution.Scripts);

            // Validate folders.
            var internalFolder = solution.Folders.FirstOrDefault(f => f.Name == "Internal");
            Assert.IsNotNull(internalFolder);
            Assert.AreEqual("Internal", internalFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Internal"), internalFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("739B5C83-523B-4ECA-9597-E3E4F098A462"), internalFolder.Guid);
            }

            Assert.HasCount(1, internalFolder.Children);
            Assert.IsEmpty(internalFolder.Files);
            Assert.IsEmpty(internalFolder.SubProjects);
            Assert.HasCount(1, internalFolder.SubFolders);
            Assert.IsNull(internalFolder.Parent);

            var scriptsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Scripts");
            Assert.IsNotNull(scriptsFolder);
            Assert.IsNull(scriptsFolder.Parent);
            Assert.AreEqual("Scripts", scriptsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Scripts"), scriptsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("47698E4B-B6D1-430F-BE2D-BFB6682E015C"), scriptsFolder.Guid);
            }

            Assert.HasCount(1, scriptsFolder.Children);
            Assert.IsEmpty(scriptsFolder.Files);
            Assert.IsEmpty(scriptsFolder.SubProjects);
            Assert.HasCount(1, scriptsFolder.SubFolders);

            var dllsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Dlls");
            Assert.IsNotNull(dllsFolder);
            Assert.AreEqual("Dlls", dllsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Dlls"), dllsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("F5170A2F-F6C3-407D-927A-B6CCF7A03677"), dllsFolder.Guid);
            }

            Assert.IsEmpty(dllsFolder.Children, "Folder count");
            Assert.HasCount(1, dllsFolder.Files, "File count");
            Assert.IsEmpty(dllsFolder.SubProjects, "SubProject count");
            Assert.IsEmpty(dllsFolder.SubFolders, "Subfolder count");

            Assert.AreEqual("readme.txt", dllsFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Dlls", "readme.txt"), dllsFolder.Files.First().AbsolutePath);

            var documentationFolder = solution.Folders.FirstOrDefault(f => f.Name == "Documentation");
            Assert.IsNotNull(documentationFolder);
            Assert.AreEqual("Documentation", documentationFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Documentation"), documentationFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("822AB0F6-E8A6-48A2-8259-1A706A64C378"), documentationFolder.Guid);
            }

            Assert.IsEmpty(documentationFolder.Children, "Folder count");
            Assert.HasCount(1, documentationFolder.Files, "File count");
            Assert.IsEmpty(documentationFolder.SubProjects, "SubProject count");
            Assert.IsEmpty(documentationFolder.SubFolders, "Subfolder count");

            Assert.AreEqual("readme.txt", documentationFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Documentation", "readme.txt"), documentationFolder.Files.First().AbsolutePath);

            var codeAnalysisFolder = solution.Folders.FirstOrDefault(f => f.Name == "Code Analysis");
            Assert.IsNotNull(codeAnalysisFolder);
            Assert.AreEqual("Code Analysis", codeAnalysisFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Code Analysis"), codeAnalysisFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("1954015F-1B20-4D56-99FB-D3EDC30CFFDC"), codeAnalysisFolder.Guid);
            }

            Assert.IsEmpty(codeAnalysisFolder.Children, "Folder count");
            Assert.AreEqual(2, codeAnalysisFolder.Files.Count(), "File count");
            Assert.IsEmpty(codeAnalysisFolder.SubProjects, "SubProject count");
            Assert.IsEmpty(codeAnalysisFolder.SubFolders, "Subfolder count");

            Assert.AreEqual("qaction-debug.ruleset", codeAnalysisFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Internal", "Code Analysis", "qaction-debug.ruleset"), codeAnalysisFolder.Files.First().AbsolutePath);

            var clearableAlarmsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Clearable Alarms");
            Assert.IsNotNull(clearableAlarmsFolder);
            Assert.AreEqual("Clearable Alarms", clearableAlarmsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Clearable Alarms"), clearableAlarmsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("92BA8B5A-664F-40A0-878F-B12B8AD55AA6"), clearableAlarmsFolder.Guid);
            }

            Assert.HasCount(1, clearableAlarmsFolder.Children);
            Assert.HasCount(1, clearableAlarmsFolder.Files);
            Assert.IsEmpty(clearableAlarmsFolder.SubProjects);
            Assert.HasCount(1, clearableAlarmsFolder.SubFolders);

            Assert.AreEqual("Clearable Alarms.xml", clearableAlarmsFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Clearable Alarms.xml"), clearableAlarmsFolder.Files.First().AbsolutePath);

            var actionsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Actions");
            Assert.IsNotNull(actionsFolder);
            Assert.AreEqual("Actions", actionsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Actions"), actionsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("EF859381-A300-4BEA-ABFD-6D85B0E4D127"), actionsFolder.Guid);
            }

            Assert.HasCount(1, actionsFolder.Children);
            Assert.IsEmpty(actionsFolder.Files);
            Assert.HasCount(1, actionsFolder.SubProjects);
            Assert.IsEmpty(actionsFolder.SubFolders);

            var project1 = solution.Projects.FirstOrDefault(p => p.Name == "Clearable Alarms_1");
            Assert.AreEqual("Clearable Alarms_1", project1.Name);
            Assert.AreEqual(@"Clearable Alarms_1\Clearable Alarms_1.csproj", project1.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, @"Clearable Alarms_1\Clearable Alarms_1.csproj"), project1.AbsolutePath);
            Assert.AreEqual("Actions", project1.Parent.Name);

            // Verify folder structure
            Assert.AreEqual(codeAnalysisFolder, internalFolder.Children.First());
            Assert.AreEqual(codeAnalysisFolder, internalFolder.SubFolders.First());

            Assert.AreEqual(clearableAlarmsFolder, scriptsFolder.Children.First());
            Assert.AreEqual(clearableAlarmsFolder, scriptsFolder.SubFolders.First());

            Assert.AreEqual(actionsFolder, clearableAlarmsFolder.Children.First());
            Assert.AreEqual(actionsFolder, clearableAlarmsFolder.SubFolders.First());

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Clearable Alarms").Script;
            Assert.IsNotNull(script1);
            Assert.HasCount(1, script1.ScriptExes);
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Clearable Alarms_1]", exes1[0].Code);
        }
    }
}