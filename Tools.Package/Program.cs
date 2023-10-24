namespace Skyline.DataMiner.CICD.Tools.Package
{
    using System;
    using System.CommandLine;
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
                description: "Directory containing the items for the package");

            var outputDirectory = new Option<string>(
                name: "--output",
                description: "Directory where the package will be stored.")
            {
                IsRequired = true,
                ArgumentHelpName = "OUTPUT_DIRECTORY"
            };
            outputDirectory.AddAlias("-o");

            var packageName = new Option<string>(
                name: "--name",
                description: "Name for the package.",
                getDefaultValue: () => "Package")
            {
                IsRequired = false,
                ArgumentHelpName = "OUTPUT_NAME"
            };
            packageName.AddAlias("-n");

            var rootCommand = new RootCommand("This .NET tool allows you to create dmapp and dmprotocol packages.");
            rootCommand.AddGlobalOption(debugOption);
            rootCommand.AddGlobalOption(outputDirectory);
            rootCommand.AddGlobalOption(packageName);

            var dmprotocolSubCommand = new Command("dmprotocol", "Creates a dmprotocol package based on a protocol solution")
            {
                workspaceArgument
            };
            dmprotocolSubCommand.SetHandler(ProcessDmProtocolAsync, workspaceArgument, outputDirectory, packageName, debugOption);

            var dmappType = new Option<string>(
                name: "--type",
                description: "Type of dmapp package.")
            {
                IsRequired = true,
            }.FromAmong("automation", "visio", "dashboard", "protocolvisio");
            dmappType.AddAlias("-t");

            var buildNumber = new Option<uint>(
                name: "--build-number",
                description: "The build number of a workflow run.")
            {
                ArgumentHelpName = "BUILD_NUMBER"
            };
            buildNumber.AddAlias("-bn");

            var version = new Option<string>(
                name: "--version",
                description: "The version number for the artifact. This takes precedent before 'build-number'. Supported formats: 'A.B.C', 'A.B.C.D', 'A.B.C-suffix' and 'A.B.C.D-suffix'.")
            {
                ArgumentHelpName = "VERSION",
            };
            version.AddAlias("-v");

            var protocolName = new Option<string>(
                name: "--protocolName",
                description: "The protocol name. Only applicable for the 'protocolvisio' type.")
            {
                ArgumentHelpName = "PROTOCOL_NAME"
            };
            protocolName.AddAlias("-pn");

            var dmappSubCommand = new Command("dmapp", "Create a dmapp package based on the type.")
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
            if (String.IsNullOrWhiteSpace(workspace))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(workspace));
            }

            if (String.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(outputDirectory));
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
            }

            if (String.IsNullOrWhiteSpace(dmappType))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(dmappType));
            }

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
                    if (String.IsNullOrWhiteSpace(protocolName))
                    {
                        throw new ArgumentException("Value cannot be null or whitespace.", nameof(protocolName));
                    }

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
        }

        private static async Task ProcessDmProtocolAsync(string workspace, string outputDirectory, string packageName, bool debug)
        {
            if (String.IsNullOrWhiteSpace(workspace))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(workspace));
            }

            if (String.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(outputDirectory));
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
            }

            IAppPackageProtocol package = await ProtocolPackageCreator.Factory.FromRepositoryAsync(new Logging(debug), workspace);
            package.CreatePackage(FileSystem.Instance.Path.Combine(outputDirectory, packageName + ".dmprotocol"));
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