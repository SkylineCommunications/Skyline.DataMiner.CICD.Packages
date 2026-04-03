namespace Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects
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