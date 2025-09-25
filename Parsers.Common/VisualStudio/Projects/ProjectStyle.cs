namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    /// <summary>
    /// Style of the project.
    /// </summary>
    public enum ProjectStyle
    {
        /// <summary>
        /// Unable to verify the style.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Legacy style.
        /// </summary>
        Legacy,

        /// <summary>
        /// SDK style.
        /// </summary>
        Sdk,
    }
}