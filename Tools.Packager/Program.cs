namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.DMApp.Dashboard;
    using Skyline.DataMiner.CICD.DMApp.Visio;
    using Skyline.DataMiner.CICD.DMProtocol;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Tools.Reporter;

    /// <summary>
    /// This .NET tool allows you to create dmapp and dmprotocol packages..
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Code that will be called when running the tool.
        /// </summary>
        /// <param name="args">Extra arguments.</param>
        /// <returns>0 if successful.</returns>
        public static async Task<int> Main(string[] args)
        {
            var debugOption = new Option<bool>(
                name: "--debug",
                description: "Enable debug logging.")
            {
                IsHidden = true
            };

            var workspaceArgument = new Argument<string>(
                name: "directory",
                description: "Directory containing the package items");
            workspaceArgument.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForArgument(workspaceArgument)))
                {
                    result.ErrorMessage = "Directory can not be null, empty or whitespace.";
                }
            });

            var outputDirectory = new Option<string>(
                name: "--output",
                description: "Directory where the package will be stored.")
            {
                IsRequired = true,
                ArgumentHelpName = "OUTPUT_DIRECTORY"
            };
            outputDirectory.AddAlias("-o");
            outputDirectory.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(outputDirectory)))
                {
                    result.ErrorMessage = "Output can not be null, empty or whitespace.";
                }
            });

            var packageName = new Option<string>(
                name: "--name",
                description: "Name of the package.")
            {
                IsRequired = false,
                ArgumentHelpName = "OUTPUT_NAME"
            };
            packageName.AddAlias("-n");
            packageName.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(packageName)))
                {
                    result.ErrorMessage = "Package name can not be null, empty or whitespace.";
                }
            });

            var rootCommand = new RootCommand("This .NET tool allows you to create DataMiner application (.dmapp) and protocol (.dmprotocol) packages.");
            rootCommand.AddGlobalOption(debugOption);
            rootCommand.AddGlobalOption(outputDirectory);
            rootCommand.AddGlobalOption(packageName);

            var versionOverride = new Option<string>(
                name: "--version-override",
                description: "Override the version in the protocol.")
            {
                ArgumentHelpName = "VERSION_OVERRIDE",
            };
            versionOverride.AddAlias("-vo");
            versionOverride.AddValidator(result =>
            {
                var value = result.GetValueForOption(versionOverride);

                if (String.IsNullOrWhiteSpace(value))
                {
                    result.ErrorMessage = "Version can not be null, empty or whitespace.";
                    return;
                }
            });

            var dmprotocolSubCommand = new Command("dmprotocol", "Creates a protocol package (.dmprotocol) based on a protocol solution.")
            {
                workspaceArgument,
                versionOverride,
            };
            dmprotocolSubCommand.SetHandler(ProcessDmProtocolAsync, workspaceArgument, outputDirectory, packageName, versionOverride, debugOption);

            var dmappType = new Option<string>(
                name: "--type",
                description: "Can be ignored for Skyline.DataMiner.Sdk Projects. In case of Legacy Solutions this defined the type of dmapp package created by the solution.")
            {
                IsRequired = false,
            }.FromAmong("automation", "visio", "dashboard", "protocolvisio", "sdk");
            dmappType.AddAlias("-t");
            dmappType.SetDefaultValue("sdk");

            var buildNumber = new Option<uint>(
                name: "--build-number",
                description: "The build number.")
            {
                ArgumentHelpName = "BUILD_NUMBER"
            };
            buildNumber.AddAlias("-bn");

            var version = new Option<string>(
                name: "--version",
                description: "The version number for the artifact. This takes precedence over 'build-number'. Supported formats: 'A.B.C', 'A.B.C.D', 'A.B.C-suffix' and 'A.B.C.D-suffix'.")
            {
                ArgumentHelpName = "VERSION",
            };
            version.AddAlias("-v");
            version.AddValidator(result =>
            {
                var value = result.GetValueForOption(version);

                if (String.IsNullOrWhiteSpace(value))
                {
                    result.ErrorMessage = "Version can not be null, empty or whitespace.";
                    return;
                }

                // regexr.com/7gcu9
                if (!Regex.IsMatch(value, "^(\\d+\\.){2,3}\\d+(-\\w+)?$"))
                {
                    result.ErrorMessage = "Invalid format. Supported formats: 'A.B.C', 'A.B.C.D', 'A.B.C-suffix' and 'A.B.C.D-suffix'.";
                }
            });

            var protocolName = new Option<string>(
                name: "--protocol-name",
                description: "The protocol name. Only applicable for the 'protocolvisio' type.")
            {
                ArgumentHelpName = "PROTOCOL_NAME"
            };
            protocolName.AddAlias("-pn");
            protocolName.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(protocolName)))
                {
                    result.ErrorMessage = "Protocol name can not be null, empty or whitespace.";
                }
            });

            var dmappSubCommand = new Command("dmapp", "Creates a DataMiner application (.dmapp) package based on the type.")
            {
                workspaceArgument,
                dmappType,
                buildNumber,
                version,
                protocolName
            };

            dmappSubCommand.SetHandler(ProcessDmAppAsync, workspaceArgument, outputDirectory, packageName, debugOption, dmappType, buildNumber, version, protocolName);

            rootCommand.Add(dmappSubCommand);
            rootCommand.Add(dmprotocolSubCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task ProcessDmAppAsync(string workspace, string outputDirectory, string packageName, bool debug, string dmappType, uint buildNumber, string version, string protocolName)
        {
            if (dmappType == "sdk")
            {
                var fs = FileSystem.Instance;

                // sdk type means we just perform dotnet build then move all the .dmapp created to the outputDirectory
                // Supporting multiple solutions in the same workspace here, then this tool has some additional functionalty beyond dotnet build
                var allSolutions = fs.Directory.EnumerateFiles(workspace, "*.sln", System.IO.SearchOption.AllDirectories);
                string dmappVersion;

                if (!String.IsNullOrWhiteSpace(version))
                {
                    dmappVersion = version;
                }
                else
                {
                    dmappVersion = $"0.0.{buildNumber}";
                }

                foreach (var solution in allSolutions)
                {
                    ProcessStartInfo psi;

                    // No support for --ouput or similar. BaseOutput has bad behavior, the other outputs aren't processed by the SDK
                    psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"build \"{solution}\" -p:Version={dmappVersion}",
                        UseShellExecute = false
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        Console.WriteLine($"Build exited with code {process.ExitCode}");
                    }
                }

                var allDmapps = fs.Directory.EnumerateFiles(workspace, "*.dmapp", System.IO.SearchOption.AllDirectories)
     .Where(file => file.Contains($"bin"));
                var allZips = fs.Directory.EnumerateFiles(workspace, "*.zip", System.IO.SearchOption.AllDirectories)
.Where(file => file.Contains($"bin"));

                fs.Directory.CreateDirectory(outputDirectory);

                foreach (var dmappFile in allDmapps)
                {
                    string parentDirectory = fs.Path.GetDirectoryName(dmappFile);
                    fs.File.MoveFile(dmappFile, parentDirectory, outputDirectory, true);
                }

                foreach (var zipFile in allZips)
                {
                    fs.File.MoveFile(zipFile, fs.Path.GetDirectoryName(zipFile), outputDirectory, true);
                }
            }
            else
            {
                IAppPackageCreator appPackageCreator;
                DMAppVersion dmAppVersion;

                if (!String.IsNullOrWhiteSpace(version))
                {
                    if (Regex.IsMatch(version, "^[0-9]+.[0-9]+.[0-9]+(-CU[0-9]+)?$"))
                    {
                        dmAppVersion = DMAppVersion.FromDataMinerVersion(version);
                    }
                    else if (Regex.IsMatch(version, "[0-9]+.[0-9]+.[0-9]+.[0-9]+$"))
                    {
                        dmAppVersion = DMAppVersion.FromProtocolVersion(version);
                    }
                    else
                    {
                        // Supports pre-releases
                        dmAppVersion = DMAppVersion.FromPreRelease(version);
                    }
                }
                else
                {
                    dmAppVersion = DMAppVersion.FromBuildNumber(buildNumber);
                }

                if (String.IsNullOrWhiteSpace(packageName))
                {
                    // Create default name if no custom name was used.
                    packageName = $"Package {dmAppVersion}";
                }

                switch (dmappType)
                {
                    case "automation":
                        appPackageCreator =
                            AppPackageCreatorForAutomation.Factory.FromRepository(new Logging(debug), workspace, packageName, dmAppVersion);
                        break;

                    case "visio":
                        appPackageCreator =
                            AppPackageCreatorForVisio.Factory.FromRepository(new Logging(debug), workspace, packageName, dmAppVersion);
                        break;

                    case "protocolvisio":
                        appPackageCreator = AppPackageCreatorForProtocolVisio.Factory.FromRepository(FileSystem.Instance, new Logging(debug),
                            workspace, packageName, dmAppVersion, protocolName);
                        break;

                    case "dashboard":
                        appPackageCreator =
                            AppPackageCreatorForDashboard.Factory.FromRepository(new Logging(debug), workspace, packageName, dmAppVersion);
                        break;
                    default:
                        throw new NotImplementedException($"DMApp type '{dmappType}' has not been implemented yet.");
                }

                DMAppFileName dmAppFileName = new DMAppFileName(packageName + ".dmapp");
                await appPackageCreator.CreateAsync(outputDirectory, dmAppFileName);
                await SendMetricAsync("DMAPP", dmappType);
            }
        }

        private static async Task ProcessDmProtocolAsync(string workspace, string outputDirectory, string packageName, string versionOverride, bool debug)
        {
            IAppPackageProtocol package;

            if (String.IsNullOrWhiteSpace(versionOverride))
            {
                package = await ProtocolPackageCreator.Factory.FromRepositoryAsync(new Logging(debug), workspace);
            }
            else
            {
                package = await ProtocolPackageCreator.Factory.FromRepositoryAsync(new Logging(debug), workspace, versionOverride);
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                // Create default name if no custom name was used.
                packageName = $"{package.Name} {package.Version}";
            }

            package.CreatePackage(FileSystem.Instance.Path.Combine(outputDirectory, packageName + ".dmprotocol"));
            await SendMetricAsync("DMPROTOCOL");
        }

        private static async Task SendMetricAsync(string type, string dmappType = null)
        {
            try
            {
                DevOpsMetrics metrics = new DevOpsMetrics();
                string message = $"Skyline.DataMiner.CICD.Tools.Packager|{type}";
                if (dmappType != null)
                {
                    message += $"|{dmappType}";
                }

                await metrics.ReportAsync(message);
            }
            catch
            {
                // Silently catch as if the request fails due to network issues we don't want the tool to fail.
            }
        }
    }

    internal class Logging : ILogCollector
    {
        private readonly bool debug;

        public Logging(bool debug)
        {
            this.debug = debug;
        }

        public void ReportError(string error)
        {
            ReportLog($"ERROR|{error}");
        }

        public void ReportStatus(string status)
        {
            ReportLog($"STATUS|{status}");
        }

        public void ReportWarning(string warning)
        {
            ReportLog($"WARNING|{warning}");
        }

        public void ReportDebug(string debug)
        {
            ReportLog($"DEBUG|{debug}");
        }

        public void ReportLog(string message)
        {
            if (debug)
            {
                Console.WriteLine(message);
            }
        }
    }
}