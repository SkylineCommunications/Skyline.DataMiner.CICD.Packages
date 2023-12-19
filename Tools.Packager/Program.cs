namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.CommandLine.Parsing;
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
    using Skyline.DataMiner.Net.Connectivity;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;

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

            var metricsOption = new Option<bool>(
                name: "--allowMetrics",
                description: "Disables the collection of metrics for devopsmetrics.skyline.be",
                getDefaultValue: () => true);
            metricsOption.AddAlias("-m");

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
            rootCommand.AddGlobalOption(metricsOption);

            var versionOverride = new Option<string>(
                name: "--versionOverride",
                description: "Override the version in the protocol.")
            {
                ArgumentHelpName = "VERSION_OVERRIDE",
            };
            versionOverride.AddAlias("-vo");z
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
                description: "Type of dmapp package.")
            {
                IsRequired = true,
            }.FromAmong("automation", "visio", "dashboard", "protocolvisio");
            dmappType.AddAlias("-t");

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
                name: "--protocolName",
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
            
            dmappSubCommand.SetHandler(async context =>
            {
                var w = context.ParseResult.GetValueForArgument(workspaceArgument);
                var o = context.ParseResult.GetValueForOption(outputDirectory);
                var n = context.ParseResult.GetValueForOption(packageName);
                var debug = context.ParseResult.GetValueForOption(debugOption);
                var t = context.ParseResult.GetValueForOption(dmappType);
                var bn = context.ParseResult.GetValueForOption(buildNumber);
                var v = context.ParseResult.GetValueForOption(version);
                var pn = context.ParseResult.GetValueForOption(protocolName);
                var m = context.ParseResult.GetValueForOption(metricsOption);
                await ProcessDmAppAsync(w, o, n, debug, t, bn, v, pn, m);
            });

            rootCommand.Add(dmappSubCommand);
            rootCommand.Add(dmprotocolSubCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task ProcessDmAppAsync(string workspace, string outputDirectory, string packageName, bool debug, string dmappType, uint buildNumber, string version, string protocolName, bool allowMetrics)
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
            await SendMetric(allowMetrics, "DMAPP", dmappType);
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
            await SendMetric(allowMetrics, "DMPROTOCOL");
        }

        private static async Task SendMetric(bool allowMetrics, string type, string dmappType = null)
        {
            if (!allowMetrics)
            {
                return;
            }

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