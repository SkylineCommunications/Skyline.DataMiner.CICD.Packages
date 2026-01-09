namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Aspera
{
    using System.ComponentModel;

    public enum AsperaType
    {
        [Description("Aspera Faspex")]
        Faspex = 0,

        [Description("Aspera Shares")]
        Shares = 1,
    }

    public enum AsperaWorkgroup
    {
        [Description("Messi (HELIPLAY)")]
        Messi_HELIPLAY = 0,

        [Description("Mediaputiikki (TREIPLAY)")]
        Mediaputiikki_TREIPLAY = 1,

        [Description("Mediamylly (VSAIPLAY)")]
        Mediamylly_VSAIPLAY = 2
    }

    public enum ImportDepartment
    {
        [Description("Messi (HELIPLAY)")]
        Messi_HELIPLAY = 0,

        [Description("Mediaputiikki (TREIPLAY)")]
        Mediaputiikki_TREIPLAY = 1,

        [Description("Mediamylly (VSAIPLAY)")]
        Mediamylly_VSAIPLAY = 2
    }
}