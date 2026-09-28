
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.CICD.Assemblers.Automation;
using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;

namespace Assemblers.AutomationTests
{
    [TestClass]
    public class MSBuilderHelpersTests
    {
        [TestMethod]
        public void EvaluateReferenceProject_BasicPackableProject_ReadsCoreProperties()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            var csprojPath = Path.Combine(dir, "Lib.csproj");
            File.WriteAllText(csprojPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <AssemblyName>LibA</AssemblyName>
                    <PackageId>Pkg.LibA</PackageId>
                    <PackageVersion>1.0.5</PackageVersion>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                </Project>
                """);
            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "netstandard2.0");
            Assert.IsNotNull(info);
            Assert.AreEqual("Pkg.LibA", info.PackageId);
            Assert.AreEqual("netstandard2.0", info.TargetFramework);
            Assert.AreEqual("1.0.5", info.PackageVersion);
            Assert.IsTrue(info.ShouldHarvestAssembly());
        }
       
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_ReadsTargetFramework()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var csprojPath = Path.Combine(dir, "Lib.csproj");

                File.WriteAllText(csprojPath, """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFrameworks>net48;net10.0</TargetFrameworks>
                        <OutputType>Library</OutputType>
                        <AssemblyName>Lib</AssemblyName>
                      </PropertyGroup>
                    </Project>
                    """);

                var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");

                Assert.IsNotNull(info);
                Assert.AreEqual("Library", info.OutputType);
                Assert.AreEqual("net48", info.TargetFramework);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_ReadsTargetPath()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var csprojPath = Path.Combine(dir, "Lib.csproj");

                File.WriteAllText(csprojPath, """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFrameworks>net48;net10.0</TargetFrameworks>
                        <OutputType>Library</OutputType>
                        <AssemblyName>Lib</AssemblyName>
                      </PropertyGroup>
                    </Project>
                    """);
                var info =MSBuildHelpers.EvaluateReferenceProject(csprojPath,"net48");

                Assert.IsNotNull(info);
                Assert.AreEqual("net48", info.TargetFramework);

                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(info.TargetPath));

                Assert.IsTrue(
                    info.TargetPath.EndsWith(
                        Path.Combine("net48", "Lib.dll"),
                        StringComparison.OrdinalIgnoreCase));


            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_RecursiveReference_UsesSameTargetFramework()
        {
       
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var libraryBPath = Path.Combine(dir, "LibraryB.csproj");
                File.WriteAllText(libraryBPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net48;net10.0</TargetFrameworks>
                <OutputType>Library</OutputType>
                <AssemblyName>LibraryB</AssemblyName>
              </PropertyGroup>
            </Project>
            """);

                var libraryAPath = Path.Combine(dir, "LibraryA.csproj");
                File.WriteAllText(libraryAPath, $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net48;net10.0</TargetFrameworks>
                <OutputType>Library</OutputType>
                <AssemblyName>LibraryA</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="{libraryBPath}" />
              </ItemGroup>
            </Project>
            """);

                var rootPath = Path.Combine(dir, "Root.csproj");
                File.WriteAllText(rootPath, $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <OutputType>Library</OutputType>
                <AssemblyName>Root</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="{libraryAPath}" />
              </ItemGroup>
            </Project>
            """);

                var projectCollection = new ProjectCollection();

                var rootProject = projectCollection.LoadProject(
                    rootPath,
                    new Dictionary<string, string>
                    {
                        ["TargetFramework"] = "net48",
                    },
                    null);

                var rootTfm = rootProject.GetPropertyValue("TargetFramework");

                var libraryA = MSBuildHelpers.EvaluateReferenceProject(
                    libraryAPath,
                    rootTfm);

                Assert.IsNotNull(libraryA);
                Assert.AreEqual("net48", libraryA.TargetFramework);
                StringAssert.EndsWith(libraryA.TargetPath, @"bin\Debug\net48\LibraryA.dll");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
    
    }
