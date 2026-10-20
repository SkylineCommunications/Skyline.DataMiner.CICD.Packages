namespace Assemblers.ProtocolTests.DependencyResolution
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.DependencyResolution;

    [TestClass]
    public class MarkerEvaluatorTests
    {
        private static readonly IReadOnlyDictionary<string, string> WindowsEnvironment = new Dictionary<string, string>
        {
            ["sys_platform"] = "win32",
            ["os_name"] = "nt",
            ["platform_system"] = "Windows",
            ["python_version"] = "3.14",
        };

        private static readonly IReadOnlyDictionary<string, string> LinuxEnvironment = new Dictionary<string, string>
        {
            ["sys_platform"] = "linux",
            ["os_name"] = "posix",
            ["platform_system"] = "Linux",
            ["python_version"] = "3.14",
        };

        [TestMethod]
        public void Evaluate_NoMarker_ReturnsTrue()
        {
            MarkerEvaluator.Evaluate(String.Empty, WindowsEnvironment).Should().BeTrue();
            MarkerEvaluator.Evaluate(null, WindowsEnvironment).Should().BeTrue();
        }

        [TestMethod]
        public void Evaluate_SimpleEquality_MatchesOnlyTargetPlatform()
        {
            const string marker = "sys_platform == \"win32\"";

            MarkerEvaluator.Evaluate(marker, WindowsEnvironment).Should().BeTrue();
            MarkerEvaluator.Evaluate(marker, LinuxEnvironment).Should().BeFalse();
        }

        [TestMethod]
        public void Evaluate_Inequality_MatchesOppositePlatform()
        {
            const string marker = "sys_platform != \"win32\"";

            MarkerEvaluator.Evaluate(marker, WindowsEnvironment).Should().BeFalse();
            MarkerEvaluator.Evaluate(marker, LinuxEnvironment).Should().BeTrue();
        }

        [TestMethod]
        public void Evaluate_VersionComparison_ComparesNumerically()
        {
            MarkerEvaluator.Evaluate("python_version >= \"3.8\"", WindowsEnvironment).Should().BeTrue();
            MarkerEvaluator.Evaluate("python_version >= \"3.20\"", WindowsEnvironment).Should().BeFalse();
        }

        [TestMethod]
        public void Evaluate_NotIn_NegatesMembershipCheck()
        {
            const string marker = "os_name not in \"nt\"";

            MarkerEvaluator.Evaluate(marker, WindowsEnvironment).Should().BeFalse();
            MarkerEvaluator.Evaluate(marker, LinuxEnvironment).Should().BeTrue();
        }

        [TestMethod]
        public void Evaluate_AndExpression_ThrowsNotSupported()
        {
            const string marker = "sys_platform == \"linux\" and python_version >= \"3.8\"";

            Action act = () => MarkerEvaluator.Evaluate(marker, LinuxEnvironment);

            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void Evaluate_OrExpression_ThrowsNotSupported()
        {
            const string marker = "sys_platform == \"win32\" or sys_platform == \"linux\"";

            Action act = () => MarkerEvaluator.Evaluate(marker, WindowsEnvironment);

            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void Evaluate_ParenthesizedExpression_ThrowsNotSupported()
        {
            const string marker = "(sys_platform == \"win32\")";

            Action act = () => MarkerEvaluator.Evaluate(marker, WindowsEnvironment);

            act.Should().Throw<NotSupportedException>();
        }
    }
}
