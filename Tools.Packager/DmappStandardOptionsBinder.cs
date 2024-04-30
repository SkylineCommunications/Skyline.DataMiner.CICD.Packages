namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System.CommandLine;
    using System.CommandLine.Binding;

    /// <summary>
    /// Represents a collection of arguments required for standard dmapp data.
    /// </summary>
    internal class DmappStandardOptionsBinder : BinderBase<StandardDmappOptions>
    {
        private readonly Option<uint> buildNumber;
        private readonly Option<string> dmappType;
        private readonly Option<string> outputDirectory;
        private readonly Option<string> packageName;
        private readonly Option<string> version;
        private readonly Argument<string> workspace;

        /// <summary>
        /// Binds command line options to <see cref="StandardDmappOptions"/>.
        /// </summary>
        public DmappStandardOptionsBinder(Argument<string> workspace, Option<string> outputDirectory, Option<string> packageName, Option<string> dmappType, Option<string> version, Option<uint> buildnumber)
        {
            this.workspace = workspace;
            this.outputDirectory = outputDirectory;
            this.packageName = packageName;
            this.dmappType = dmappType;
            this.version = version;
            this.buildNumber = buildnumber;
        }

        /// <summary>
        /// Retrieves the bound value of <see cref="StandardDmappOptions"/> from the <see cref="BindingContext"/>.
        /// </summary>
        /// <param name="bindingContext">The context containing parsed command line arguments.</param>
        /// <returns>An instance of <see cref="StandardDmappOptions"/> populated with values obtained from the command line options.</returns>
        /// <remarks>
        /// This method overrides the base <see cref="BinderBase{T}.GetBoundValue"/> method to provide specific logic for binding command line options to the properties of <see cref="StandardDmappOptions"/>.
        /// It extracts values for each option defined in the command line arguments and assigns them to the corresponding properties of a new <see cref="StandardDmappOptions"/> instance.
        /// </remarks>
        protected override StandardDmappOptions GetBoundValue(BindingContext bindingContext)
        {
            return new StandardDmappOptions
            {
                Workspace = bindingContext.ParseResult.GetValueForArgument(workspace),
                OutputDirectory = bindingContext.ParseResult.GetValueForOption(outputDirectory),
                PackageName = bindingContext.ParseResult.GetValueForOption(packageName),
                DmappType = bindingContext.ParseResult.GetValueForOption(dmappType),
                Version = bindingContext.ParseResult.GetValueForOption(version),
                BuildNumber = bindingContext.ParseResult.GetValueForOption(buildNumber),
            };
        }
    }

    internal class StandardDmappOptions
    {
        public uint BuildNumber { get; set; }

        public string DmappType { get; set; }

        public string OutputDirectory { get; set; }

        public string PackageName { get; set; }

        public string Version { get; set; }

        public string Workspace { get; set; }
    }
}