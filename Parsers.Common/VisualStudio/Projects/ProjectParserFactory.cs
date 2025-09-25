namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System.Xml.Linq;

    internal class ProjectParserFactory
    {
        public static IProjectParser GetParser(XDocument document, string projectDir)
        {
            var sdkAttribute = document.Root.Attribute("Sdk");

            if (sdkAttribute != null)
            {
                return new SdkStyleParser(document, projectDir);
            }

            var sdkElement = document.Root.Element("Sdk");

            if (sdkElement != null)
            {
                return new SdkStyleParser(document, projectDir);
            }

            // https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#project-files
            return new LegacyStyleParser(document, projectDir);
        }
    }
}
