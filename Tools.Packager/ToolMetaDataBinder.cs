namespace Skyline.DataMiner.CICD.Tools.Packager
{
    using System.CommandLine;
    using System.CommandLine.Binding;
    using Skyline.DataMiner.CICD.DMApp.Keystone;

    /// <summary>
    /// Represents a collection of arguments required for keystones.
    /// </summary>
    internal class ToolMetaDataBinder : BinderBase<ToolMetaData>
    {
        private readonly Option<string> authors;
        private readonly Option<string> company;
        private readonly Option<string> toolCommand;
        private readonly Option<string> toolName;
        private readonly Option<string> toolVersion;
        private readonly Option<string> outputPath;

        /// <summary>
        /// Binds command line options to <see cref="ToolMetaData"/>.
        /// </summary>
        public ToolMetaDataBinder(Option<string> authors, Option<string> company, Option<string> toolCommand, Option<string> toolName, Option<string> toolVersion, Option<string> outputPath)
        {
            this.authors = authors;
            this.company = company;
            this.toolCommand = toolCommand;
            this.toolName = toolName;
            this.toolVersion = toolVersion;
            this.outputPath = outputPath;
        }

        /// <summary>
        /// Retrieves the bound value of <see cref="ToolMetaData"/> from the <see cref="BindingContext"/>.
        /// </summary>
        /// <param name="bindingContext">The context containing parsed command line arguments.</param>
        /// <returns>An instance of <see cref="ToolMetaData"/> populated with values obtained from the command line options.</returns>
        /// <remarks>
        /// This method overrides the base <see cref="BinderBase{T}.GetBoundValue"/> method to provide specific logic for binding command line options to the properties of <see cref="ToolMetaData"/>.
        /// It extracts values for each option defined in the command line arguments and assigns them to the corresponding properties of a new <see cref="ToolMetaData"/> instance.
        /// </remarks>
        protected override ToolMetaData GetBoundValue(BindingContext bindingContext)
        {
            return new ToolMetaData(
                bindingContext.ParseResult.GetValueForOption(toolCommand),
                bindingContext.ParseResult.GetValueForOption(toolName),
                bindingContext.ParseResult.GetValueForOption(toolVersion),
                bindingContext.ParseResult.GetValueForOption(company),
                bindingContext.ParseResult.GetValueForOption(authors),
                bindingContext.ParseResult.GetValueForOption(outputPath)
            );
        }
    }
}