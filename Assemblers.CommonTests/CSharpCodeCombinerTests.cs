namespace Assemblers.CommonTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    [TestClass]
    public class CSharpCodeCombinerTests
    {
        [TestMethod]
        public void CombineFiles_Null_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => CSharpCodeCombiner.CombineFiles(null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void CombineFiles_EmptyList()
        {
            // Arrange

            // Act
            var result = CSharpCodeCombiner.CombineFiles(new List<ProjectFile>(0));

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CombineFiles_SingleFile()
        {
            // Arrange
            const string content = "ABC";
            List<ProjectFile> files = new List<ProjectFile>
            {
                new ProjectFile("MyProjectFile", content)
            };

            // Act
            var result = CSharpCodeCombiner.CombineFiles(files);

            // Assert
            result.Should().BeEquivalentTo(content);
        }

        [TestMethod]
        public void CombineFiles_MultipleFiles_RandomText()
        {
            // Arrange
            const string contentFile1 = "ABC";
            const string contentFile2 = "DEF";
            List<ProjectFile> files = new List<ProjectFile>
            {
                new ProjectFile("MyProjectFile1", contentFile1),
                new ProjectFile("MyProjectFile2", contentFile2)
            };

            StringBuilder sb = new StringBuilder();
            sb.AppendLine() // usings but there aren't any
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile1")
              .AppendLine("//---------------------------------")
              .AppendLine(contentFile1)
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile2")
              .AppendLine("//---------------------------------")
              .Append(contentFile2);
            string expectedResult = sb.ToString();

            // Act
            var result = CSharpCodeCombiner.CombineFiles(files);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void CombineFiles_MultipleFiles_ActualCSharp_WithoutNamespaces()
        {
            // Arrange
            const string contentFile1 = @"using System;

public static class MyExtensions
{
    public static string MyMethod()
    {
        return String.Empty;
    }
}
";
            const string contentFile2 = @"using System;
using System.IO;

public static class MyOtherExtensions
{
    public static bool MyOtherMethod(string path)
    {
        return Path.HasExtension(path);
    }
}";
            List<ProjectFile> files = new List<ProjectFile>
            {
                new ProjectFile("MyProjectFile1", contentFile1),
                new ProjectFile("MyProjectFile2", contentFile2)
            };

            const string expectedContentFile1 = @"public static class MyExtensions
{
    public static string MyMethod()
    {
        return String.Empty;
    }
}
";
            const string expectedContentFile2 = @"public static class MyOtherExtensions
{
    public static bool MyOtherMethod(string path)
    {
        return Path.HasExtension(path);
    }
}";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;" + Environment.NewLine + "using System.IO;")
              .AppendLine()
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile1")
              .AppendLine("//---------------------------------")
              .AppendLine()
              .AppendLine(expectedContentFile1)
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile2")
              .AppendLine("//---------------------------------")
              .AppendLine()
              .Append(expectedContentFile2);
            string expectedResult = sb.ToString();

            // Act
            var result = CSharpCodeCombiner.CombineFiles(files);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void CombineFiles_MultipleFiles_ActualCSharp_WithNamespaces()
        {
            // Arrange
            const string contentFile1 = @"namespace Skyline.Protocol
{
    using System;
    
    public static class MyExtensions
    {
        public static string MyMethod()
        {
            return String.Empty;
        }
    }
}";
            const string contentFile2 = @"namespace Skyline.Protocol.Files
{
    using System.IO;

    public static class MyOtherExtensions
    {
        public static bool MyOtherMethod(string path)
        {
            return Path.HasExtension(path);
        }
    }
}";
            List<ProjectFile> files = new List<ProjectFile>
            {
                new ProjectFile("MyProjectFile1", contentFile1),
                new ProjectFile("MyProjectFile2", contentFile2)
            };

            StringBuilder sb = new StringBuilder();
            sb.AppendLine()
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile1")
              .AppendLine("//---------------------------------")
              .AppendLine(contentFile1)
              .AppendLine("//---------------------------------")
              .AppendLine("// MyProjectFile2")
              .AppendLine("//---------------------------------")
              .Append(contentFile2);
            string expectedResult = sb.ToString();

            // Act
            var result = CSharpCodeCombiner.CombineFiles(files);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}