namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.DMApp.Dashboard;
    using Skyline.DataMiner.CICD.DMApp.Keystone;
    using Skyline.DataMiner.CICD.DMApp.Visio;
    using Skyline.DataMiner.CICD.DMProtocol;
    using Skyline.DataMiner.CICD.FileSystem;
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
                description: "Type of dmapp package.")
            {
                IsRequired = true,
            }.FromAmong("automation", "visio", "dashboard", "protocolvisio", "keystone");
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

            var toolCommand = new Option<string>(
            name: "--keystone-wrapped-command",
            description: "When installed you can invoke the keystone from CLI with this command. Only applicable for the 'keystone' type not pointing to a .nupkg.")
            {
                ArgumentHelpName = "KEYSTONE_COMMAND"
            };
            toolCommand.AddAlias("-kcmd");
            toolCommand.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(toolCommand)))
                {
                    result.ErrorMessage = "Keystone command can not be null, empty or whitespace.";
                }
            });

            var toolAuthors = new Option<string>(
            name: "--keystone-wrapped-authors",
            description: "Who were the authors. Only applicable for the 'keystone' type not pointing to a .nupkg.")
            {
                ArgumentHelpName = "KEYSTONE_AUTHORS"
            };
            toolAuthors.AddAlias("-ka");
            toolAuthors.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(toolCommand)))
                {
                    result.ErrorMessage = "Keystone command can not be null, empty or whitespace.";
                }
            });

            var toolCompany = new Option<string>(
            name: "--keystone-wrapped-company",
            description: "Who is the publishing company. Only applicable for the 'keystone' type not pointing to a .nupkg.")
            {
                ArgumentHelpName = "KEYSTONE_COMPANY"
            };
            toolCompany.AddAlias("-kc");
            toolCompany.AddValidator(result =>
            {
                if (String.IsNullOrWhiteSpace(result.GetValueForOption(toolCommand)))
                {
                    result.ErrorMessage = "Keystone command can not be null, empty or whitespace.";
                }
            });

            var dmappSubCommand = new Command("dmapp", "Creates a DataMiner application (.dmapp) package based on the type.")
            {
                workspaceArgument,
                dmappType,
                buildNumber,
                version,
                protocolName,
                toolAuthors,
                toolCompany,
                toolCommand
            };

            dmappSubCommand.SetHandler(ProcessDmAppAsync, new DmappStandardOptionsBinder(workspaceArgument, outputDirectory, packageName, dmappType, version, buildNumber), debugOption, protocolName, new ToolMetaDataBinder(toolAuthors, toolCompany, toolCommand, packageName, version, outputDirectory));

            rootCommand.Add(dmappSubCommand);
            rootCommand.Add(dmprotocolSubCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task ProcessDmAppAsync(StandardDmappOptions opt, bool debug, string protocolName, ToolMetaData metaData)
        {
            IAppPackageCreator appPackageCreator;
            DMAppVersion dmAppVersion;

            if (!String.IsNullOrWhiteSpace(opt.Version))
            {
                if (Regex.IsMatch(opt.Version, "^[0-9]+.[0-9]+.[0-9]+(-CU[0-9]+)?$"))
                {
                    dmAppVersion = DMAppVersion.FromDataMinerVersion(opt.Version);
                }
                else if (Regex.IsMatch(opt.Version, "[0-9]+.[0-9]+.[0-9]+.[0-9]+$"))
                {
                    dmAppVersion = DMAppVersion.FromProtocolVersion(opt.Version);
                }
                else
                {
                    // Supports pre-releases
                    dmAppVersion = DMAppVersion.FromPreRelease(opt.Version);
                }
            }
            else
            {
                dmAppVersion = DMAppVersion.FromBuildNumber(opt.BuildNumber);
            }

            if (String.IsNullOrWhiteSpace(opt.PackageName))
            {
                // Create default name if no custom name was used.
                opt.PackageName = $"Package {dmAppVersion}";
            }

            switch (opt.DmappType)
            {
                case "automation":
                    appPackageCreator =
                        AppPackageCreatorForAutomation.Factory.FromRepository(new Logging(debug), opt.Workspace, opt.PackageName, dmAppVersion);
                    break;

                case "visio":
                    appPackageCreator =
                        AppPackageCreatorForVisio.Factory.FromRepository(new Logging(debug), opt.Workspace, opt.PackageName, dmAppVersion);
                    break;

                case "protocolvisio":
                    appPackageCreator = AppPackageCreatorForProtocolVisio.Factory.FromRepository(FileSystem.Instance, new Logging(debug),
                        opt.Workspace, opt.PackageName, dmAppVersion, protocolName);
                    break;

                case "dashboard":
                    appPackageCreator =
                        AppPackageCreatorForDashboard.Factory.FromRepository(new Logging(debug), opt.Workspace, opt.PackageName, dmAppVersion);
                    break;

                case "keystone":
                    appPackageCreator =
                        AppPackageCreatorForKeystone.Factory.FromRepository(metaData, FileSystem.Instance, new Logging(debug), opt.Workspace, opt.PackageName, dmAppVersion);
                    break;

                default:
                    throw new NotImplementedException($"DMApp type '{opt.DmappType}' has not been implemented yet.");
            }

            DMAppFileName dmAppFileName = new DMAppFileName(opt.PackageName + ".dmapp");
            await appPackageCreator.CreateAsync(opt.OutputDirectory, dmAppFileName);
            await SendMetricAsync("DMAPP", opt.DmappType);
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
}