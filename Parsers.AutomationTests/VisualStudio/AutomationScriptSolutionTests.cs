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
        #region Solution 1 (Basic)

        [TestMethod]
        public void AutomationScriptCompiler_Solution1_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.AreEqual(3, solution.Projects.Count());
            Assert.AreEqual(2, solution.Scripts.Count());

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.AreEqual(2, script1.ScriptExes.Count());
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll" }, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.AreEqual(1, script2.ScriptExes.Count());
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 2 (Scripts in subfolders)

        [TestMethod]
        public void AutomationScriptCompiler_Solution2_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solution2"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.AreEqual(3, solution.Projects.Count());
            Assert.AreEqual(2, solution.Scripts.Count());

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.AreEqual(2, script1.ScriptExes.Count());
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll" }, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.AreEqual(1, script2.ScriptExes.Count());
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 3 (SharedProject)

        [TestMethod]
        public void AutomationScriptCompiler_Solution3_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solution3"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.AreEqual(3, solution.Projects.Count());
            Assert.AreEqual(2, solution.Scripts.Count());

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_1").Script;
            Assert.IsNotNull(script1);
            Assert.AreEqual(2, script1.ScriptExes.Count());
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:Script_1a]", exes1[0].Code);
            Assert.AreEqual("[Project:Script_1b]", exes1[1].Code);
            CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll" }, exes1[0].DllImports.ToArray());

            var script2 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "Script_2").Script;
            Assert.IsNotNull(script2);
            Assert.AreEqual(1, script2.ScriptExes.Count());
            Assert.AreEqual("[Project:Script_2]", script2.ScriptExes.First().Code);
        }

        #endregion

        #region Solution 4 (Exception)

        [TestMethod]
        public void AutomationScriptCompiler_Solution4_Load()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solution4"));
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
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solution5"));
            var path = FileSystem.Instance.Path.Combine(dir, "AutomationScript.sln");

            var solution = AutomationScriptSolution.Load(path);

            Assert.IsInstanceOfType(solution, typeof(AutomationScriptSolution));

            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            Assert.AreEqual(1, solution.Projects.Count());
            Assert.AreEqual(1, solution.Scripts.Count());

            var script1 = solution.Scripts.FirstOrDefault(s => s.Script.Name == "New").Script;
            Assert.IsNotNull(script1);
            Assert.AreEqual(1, script1.ScriptExes.Count());
            var exes1 = script1.ScriptExes.ToList();
            Assert.AreEqual("[Project:New_1]", exes1[0].Code);
            //CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll" }, exes1[0].DllImports.ToArray());
        }
    }
}