namespace Parsers.ProtocolTests.VisualStudio
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Protocol.VisualStudio;

    [TestClass]
    public class ProtocolSolutionTests
    {
        [TestMethod]
        public void ProtocolSolution_Solution_Load_Legacy()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            var solution = ProtocolSolution.Load(path);

            ValidateSolution(solution, dir, path, false);
        }

        [TestMethod]
        public void ProtocolSolution_Solution_Load_Slnx()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.slnx");

            var solution = ProtocolSolution.Load(path);

            ValidateSolution(solution, dir, path, true);
        }

        private static void ValidateSolution(ProtocolSolution solution, string dir, string path, bool isSlnx)
        {
            solution.Should().NotBeNull();
            solution.SolutionPath.Should().BeEquivalentTo(path);
            solution.SolutionDirectory.Should().BeEquivalentTo(FileSystem.Instance.Path.GetDirectoryName(path));

            solution.Folders.Should().HaveCount(3);
            solution.Projects.Should().HaveCount(6);
            solution.QActions.Should().HaveCount(5);

            // Validate folders.
            // Internal folder.
            var internalFolder = solution.Folders.FirstOrDefault(f => f.Name == "Internal");
            Assert.IsNotNull(internalFolder);
            Assert.AreEqual("Internal", internalFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Internal"), internalFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("07EA3993-AA2E-4C59-9110-BF25DB8450BE"), internalFolder.Guid);
            }

            Assert.HasCount(1, internalFolder.Children);
            Assert.IsEmpty(internalFolder.Files);
            Assert.HasCount(1, internalFolder.SubProjects);
            Assert.IsEmpty(internalFolder.SubFolders);
            Assert.IsNull(internalFolder.Parent);

            Assert.AreEqual("QAction_Helper", internalFolder.Children.First().Name);

            Assert.AreEqual("QAction_Helper", internalFolder.SubProjects.First().Name);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_Helper", "QAction_Helper.csproj"), internalFolder.SubProjects.First().AbsolutePath);
            Assert.AreEqual(@"QAction_Helper\QAction_Helper.csproj", internalFolder.SubProjects.First().RelativePath);
            Assert.AreEqual(internalFolder, internalFolder.SubProjects.First().Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("31B1EF6A-2E94-4F70-9B05-F297AB3B6C69"), internalFolder.SubProjects.First().Guid);
            }

            // QActions folder.
            var qactionsFolder = solution.Folders.FirstOrDefault(f => f.Name == "QActions");
            Assert.IsNotNull(qactionsFolder);
            Assert.IsNull(qactionsFolder.Parent);
            Assert.AreEqual("QActions", qactionsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QActions"), qactionsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("F32C5E72-BF2C-45CF-AAB1-DADCC040E51C"), qactionsFolder.Guid);
            }

            Assert.HasCount(4, qactionsFolder.Children);
            Assert.IsEmpty(qactionsFolder.Files);
            Assert.HasCount(4, qactionsFolder.SubProjects);
            Assert.IsEmpty(qactionsFolder.SubFolders);
            
            var qaction1 = solution.Projects.FirstOrDefault(p => p.Name == "QAction_1");

            Assert.IsNotNull(qaction1);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_1", "QAction_1.csproj"), qaction1.AbsolutePath);
            Assert.AreEqual(@"QAction_1\QAction_1.csproj", qaction1.RelativePath);
            Assert.AreEqual(qactionsFolder, qaction1.Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("20481214-4655-4C51-97AA-5DA92296CBCF"), qaction1.Guid);
            }

            var qaction2 = solution.Projects.FirstOrDefault(p => p.Name == "QAction_2");

            Assert.IsNotNull(qaction2);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_2", "QAction_2.csproj"), qaction2.AbsolutePath);
            Assert.AreEqual(@"QAction_2\QAction_2.csproj", qaction2.RelativePath);
            Assert.AreEqual(qactionsFolder, qaction2.Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("B5ED3E0A-72ED-42ED-8375-65925907F2D9"), qaction2.Guid);
            }

            var qaction3 = solution.Projects.FirstOrDefault(p => p.Name == "QAction_3");

            Assert.IsNotNull(qaction3);
            Assert.AreEqual("QAction_3", qaction3.Name);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_3", "QAction_3.csproj"), qaction3.AbsolutePath);
            Assert.AreEqual(@"QAction_3\QAction_3.csproj", qaction3.RelativePath);
            Assert.AreEqual(qactionsFolder, qaction3.Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("35F7A839-1F8D-4932-8850-D6B9FD17A2E8"), qaction3.Guid);
            }

            var qaction63000 = solution.Projects.FirstOrDefault(p => p.Name == "QAction_63000");

            Assert.IsNotNull(qaction63000);
            Assert.AreEqual("QAction_63000", qaction63000.Name);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_63000", "QAction_63000.csproj"), qaction63000.AbsolutePath);
            Assert.AreEqual(@"QAction_63000\QAction_63000.csproj", qaction63000.RelativePath);
            Assert.AreEqual(qactionsFolder, qaction63000.Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("113FC56A-2732-420E-B365-3915558AFD45"), qaction63000.Guid);
            }

            // Solution items folder.
            var solutionItemsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Solution Items");
            Assert.IsNotNull(solutionItemsFolder);
            Assert.IsNull(solutionItemsFolder.Parent);
            Assert.AreEqual("Solution Items", solutionItemsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Solution Items"), solutionItemsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("13998E16-2F02-4D47-BEB1-4ED034DD72D8"), solutionItemsFolder.Guid);
            }

            Assert.IsEmpty(solutionItemsFolder.Children, "Children");
            Assert.HasCount(1, solutionItemsFolder.Files, "Files");
            Assert.IsEmpty(solutionItemsFolder.SubProjects, "Subprojects");
            Assert.IsEmpty(solutionItemsFolder.SubFolders, "Subfolders");

            Assert.AreEqual("protocol.xml", solutionItemsFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "protocol.xml"), solutionItemsFolder.Files.First().AbsolutePath);

            // Check test project.
            var testProject = solution.Projects.FirstOrDefault(p => p.Name == "QAction_3Tests");
            Assert.IsNotNull(testProject);
            Assert.AreEqual("QAction_3Tests", testProject.Name);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QAction_3Tests", "QAction_3Tests.csproj"), testProject.AbsolutePath);
            Assert.AreEqual(@"QAction_3Tests\QAction_3Tests.csproj", testProject.RelativePath);
            Assert.IsNull(testProject.Parent);

            if (!isSlnx)
            {
                Assert.AreEqual(new Guid("83620144-DF9F-49E2-8376-AF72CD68B55C"), testProject.Guid);
            }

            // Verify QActions.
            var qa1 = solution.QActions.FirstOrDefault(q => q.Id == 1);
            qa1.Should().NotBeNull();
            qa1.Files.Should().HaveCount(1);
            qa1.Files[0].Code.Should().NotBeNullOrEmpty();
            qa1.DllImports.Should().BeEquivalentTo("Newtonsoft.Json.dll", "System.dll", "System.Xml.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa2 = solution.QActions.FirstOrDefault(q => q.Id == 2);
            qa2.Should().NotBeNull();
            qa2.Files.Should().HaveCount(1);
            qa2.Files[0].Code.Should().NotBeNullOrEmpty();
            qa2.DllImports.Should().BeEquivalentTo("System.dll", "[ProtocolName].[ProtocolVersion].QAction.1.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa3 = solution.QActions.FirstOrDefault(q => q.Id == 3);
            qa3.Should().NotBeNull();
            qa3.Files.Should().HaveCount(3);
            qa3.Files.All(x => !String.IsNullOrEmpty(x.Code)).Should().BeTrue();
            qa3.Files[0].Code.Should().NotBeNullOrEmpty();
            qa3.Files[1].Code.Should().NotBeNullOrEmpty();
            qa3.Files[2].Code.Should().NotBeNullOrEmpty();
            qa3.DllImports.Should().BeEquivalentTo("System.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa4 = solution.QActions.FirstOrDefault(q => q.Id == 4);
            qa4.Should().NotBeNull();
            qa4.Files.All(x => !String.IsNullOrEmpty(x.Code)).Should().BeTrue();
            qa4.DllImports.Should().BeEmpty();

            var qa63000 = solution.QActions.FirstOrDefault(q => q.Id == 63000);
            qa63000.Should().NotBeNull();
            qa63000.Files.Should().HaveCount(1);
            qa63000.Files[0].Code.Should().NotBeNullOrEmpty();
            qa63000.DllImports.Should().BeEquivalentTo("System.dll");
        }

        [TestMethod]
        public void ProtocolSolution_Solution2_Load_Legacy()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Protocol\Solution2"));
            var path = FileSystem.Instance.Path.Combine(dir, "ConnectorProtocol.sln");

            var solution = ProtocolSolution.Load(path);

            ValidateSolution2(solution, dir, path, false);
        }

        [TestMethod]
        public void ProtocolSolution_Solution2_Load_Slnx()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Protocol\Solution2"));
            var path = FileSystem.Instance.Path.Combine(dir, "ConnectorProtocol.slnx");

            var solution = ProtocolSolution.Load(path);

            ValidateSolution2(solution, dir, path, true);
        }

        private static void ValidateSolution2(ProtocolSolution solution, string dir, string path, bool isSlnx)
        {
            Assert.AreEqual(path, solution.SolutionPath);
            Assert.AreEqual(FileSystem.Instance.Path.GetDirectoryName(path), solution.SolutionDirectory);

            solution.Should().NotBeNull();
            solution.SolutionPath.Should().BeEquivalentTo(path);
            solution.SolutionDirectory.Should().BeEquivalentTo(FileSystem.Instance.Path.GetDirectoryName(path));

            solution.Folders.Should().HaveCount(8);
            solution.Projects.Should().HaveCount(3);
            solution.QActions.Should().HaveCount(2);

            // Validate folders.

            // Default templates
            var defaultTemplatesFolder = solution.Folders.FirstOrDefault(f => f.Name == "DefaultTemplates");
            Assert.IsNotNull(defaultTemplatesFolder);
            Assert.AreEqual("DefaultTemplates", defaultTemplatesFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "DefaultTemplates"), defaultTemplatesFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("F2683535-3B81-4454-9E99-120E5016BBCE"), defaultTemplatesFolder.Guid);
            }

            Assert.IsEmpty(defaultTemplatesFolder.Children);
            Assert.HasCount(1, defaultTemplatesFolder.Files);
            Assert.IsEmpty(defaultTemplatesFolder.SubProjects);
            Assert.IsEmpty(defaultTemplatesFolder.SubFolders);
            Assert.IsNull(defaultTemplatesFolder.Parent);

            Assert.AreEqual("ABOUT.md", defaultTemplatesFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "DefaultTemplates", "ABOUT.md"), defaultTemplatesFolder.Files.First().AbsolutePath);

            // Dlls
            var dllsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Dlls");
            Assert.IsNotNull(dllsFolder);
            Assert.AreEqual("Dlls", dllsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Dlls"), dllsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("90DE9AD7-DBA6-4B7D-A7E4-24CCE0BC4974"), dllsFolder.Guid);
            }

            Assert.IsEmpty(dllsFolder.Children);
            Assert.HasCount(1, dllsFolder.Files);
            Assert.IsEmpty(dllsFolder.SubProjects);
            Assert.IsEmpty(dllsFolder.SubFolders);
            Assert.IsNull(dllsFolder.Parent);

            Assert.AreEqual("ABOUT.md", dllsFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Dlls", "ABOUT.md"), dllsFolder.Files.First().AbsolutePath);

            // Documentation
            var documentationFolder = solution.Folders.FirstOrDefault(f => f.Name == "Documentation");
            Assert.IsNotNull(documentationFolder);
            Assert.AreEqual("Documentation", documentationFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Documentation"), documentationFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("C8CFB314-FE32-4349-931A-3C8A791E6918"), documentationFolder.Guid);
            }

            Assert.IsEmpty(documentationFolder.Children);
            Assert.HasCount(1, documentationFolder.Files);
            Assert.IsEmpty(documentationFolder.SubProjects);
            Assert.IsEmpty(documentationFolder.SubFolders);
            Assert.IsNull(documentationFolder.Parent);

            Assert.AreEqual("ABOUT.md", documentationFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Documentation", "ABOUT.md"), documentationFolder.Files.First().AbsolutePath);

            // Internal folder.
            var internalFolder = solution.Folders.FirstOrDefault(f => f.Name == "Internal");
            Assert.IsNotNull(internalFolder);
            Assert.AreEqual("Internal", internalFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Internal"), internalFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("E7F6E438-D81A-48C5-BC6A-2781DD80ED98"), internalFolder.Guid);
            }

            Assert.HasCount(2, internalFolder.Children);
            Assert.HasCount(1, internalFolder.Files);
            Assert.HasCount(1, internalFolder.SubProjects);
            Assert.HasCount(1, internalFolder.SubFolders);
            Assert.IsNull(internalFolder.Parent);

            Assert.AreEqual(".editorconfig", internalFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Internal", ".editorconfig"), internalFolder.Files.First().AbsolutePath);

            // Code Analysis folder.
            var codeAnalysisFolder = solution.Folders.FirstOrDefault(f => f.Name == "Code Analysis");
            Assert.IsNotNull(codeAnalysisFolder);
            Assert.AreEqual("Code Analysis", codeAnalysisFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Code Analysis"), codeAnalysisFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("90450ED1-ADC3-4799-8BCE-4C53BF325954"), codeAnalysisFolder.Guid);
            }

            Assert.IsEmpty(codeAnalysisFolder.Children);
            Assert.HasCount(3, codeAnalysisFolder.Files);
            Assert.IsEmpty(codeAnalysisFolder.SubProjects);
            Assert.IsEmpty(codeAnalysisFolder.SubFolders);
            Assert.AreEqual(internalFolder, codeAnalysisFolder.Parent);

            codeAnalysisFolder.Files.Select(f => f.FileName).Should().BeEquivalentTo("qaction-debug.ruleset", "qaction-release.ruleset", "stylecop.json");
            codeAnalysisFolder.Files.Select(f => f.AbsolutePath).Should().BeEquivalentTo(FileSystem.Instance.Path.Combine(dir, "Internal", "Code Analysis", "qaction-debug.ruleset"), FileSystem.Instance.Path.Combine(dir, "Internal", "Code Analysis", "qaction-release.ruleset"), FileSystem.Instance.Path.Combine(dir, "Internal", "Code Analysis", "stylecop.json"));

            // QActions folder.
            var qactionsFolder = solution.Folders.FirstOrDefault(f => f.Name == "QActions");
            Assert.IsNotNull(qactionsFolder);
            Assert.AreEqual("QActions", qactionsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "QActions"), qactionsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("54BF0CEF-E654-4409-A2DD-89208EFA9EC3"), qactionsFolder.Guid);
            }

            Assert.HasCount(2, qactionsFolder.Children);
            Assert.IsEmpty(qactionsFolder.Files);
            Assert.HasCount(2, qactionsFolder.SubProjects);
            Assert.IsEmpty(qactionsFolder.SubFolders);
            Assert.IsNull(qactionsFolder.Parent);
            
            // QAction Helper project.
            var qactionHelperProject = solution.Projects.FirstOrDefault(p => p.Name == "QAction_Helper");
            Assert.AreEqual("QAction_Helper", qactionHelperProject.Name);
            Assert.AreEqual(@"QAction_Helper\QAction_Helper.csproj", qactionHelperProject.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, @"QAction_Helper\QAction_Helper.csproj"), qactionHelperProject.AbsolutePath);
            Assert.AreEqual("Internal", qactionHelperProject.Parent.Name);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("3EB9EC9E-FF7B-413E-815A-8E95C3FA1288"), qactionHelperProject.Guid);
            }

            // QAction 1 project.
            var qaction1Project = solution.Projects.FirstOrDefault(p => p.Name == "QAction_1");
            Assert.AreEqual("QAction_1", qaction1Project.Name);
            Assert.AreEqual(@"QAction_1\QAction_1.csproj", qaction1Project.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, @"QAction_1\QAction_1.csproj"), qaction1Project.AbsolutePath);
            Assert.AreEqual("QActions", qaction1Project.Parent.Name);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("8C97F61C-5539-4DA1-8300-FB2344477E4F"), qaction1Project.Guid);
            }

            // QAction 2 project.
            var qaction2Project = solution.Projects.FirstOrDefault(p => p.Name == "QAction_2");
            Assert.AreEqual("QAction_2", qaction2Project.Name);
            Assert.AreEqual(@"QAction_2\QAction_2.csproj", qaction2Project.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, @"QAction_2\QAction_2.csproj"), qaction2Project.AbsolutePath);
            Assert.AreEqual("QActions", qaction2Project.Parent.Name);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("A9E60CEE-A56A-40E0-A721-0E5DE80C8283"), qaction2Project.Guid);
            }

            // Solution items folder.
            var solutionItemsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Solution Items");
            Assert.IsNotNull(solutionItemsFolder);
            Assert.IsNull(solutionItemsFolder.Parent);
            Assert.AreEqual("Solution Items", solutionItemsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Solution Items"), solutionItemsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("6A4869CC-64F1-4C38-8C6E-BD47DCAE0EFE"), solutionItemsFolder.Guid);
            }

            Assert.IsEmpty(solutionItemsFolder.Children, "Children");
            Assert.HasCount(1, solutionItemsFolder.Files, "Files");
            Assert.IsEmpty(solutionItemsFolder.SubProjects, "Subprojects");
            Assert.IsEmpty(solutionItemsFolder.SubFolders, "Subfolders");

            Assert.AreEqual("protocol.xml", solutionItemsFolder.Files.First().FileName);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "protocol.xml"), solutionItemsFolder.Files.First().AbsolutePath);

            // Tests folder.
            var testsFolder = solution.Folders.FirstOrDefault(f => f.Name == "Tests");
            Assert.IsNotNull(testsFolder);
            Assert.IsNull(testsFolder.Parent);
            Assert.AreEqual("Tests", testsFolder.RelativePath);
            Assert.AreEqual(FileSystem.Instance.Path.Combine(dir, "Tests"), testsFolder.AbsolutePath);

            if (!isSlnx)
            {
                Assert.AreEqual(Guid.Parse("146EC742-12C2-4E1F-85BB-C4C33A69B8A3"), testsFolder.Guid);
            }

            Assert.IsEmpty(testsFolder.Children, "Children");
            Assert.IsEmpty(testsFolder.Files, "Files");
            Assert.IsEmpty(testsFolder.SubProjects, "Subprojects");
            Assert.IsEmpty(testsFolder.SubFolders, "Subfolders");
            
            // Verify folder structure
            Assert.AreEqual(codeAnalysisFolder, internalFolder.Children.First());
            Assert.AreEqual(codeAnalysisFolder, internalFolder.SubFolders.First());
        }
    }
}
