namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer
{
	using System.ComponentModel;

	public enum InterplayElements
	{
		[Description("Helsinki-Interplay")]
		HELSINKI = 0,

		[Description("Tampere-Interplay")]
		TAMPERE = 1,

		[Description("Vaasa-Interplay")]
		VAASA = 2,

		[Description("UA-Interplay")]
		UA = 3
	}

	public enum SourceFileTypes
	{
		[Description("Sequence")]
		SEQUENCE = 0,

		[Description("Masterclips")]
		MASTERCLIPS = 1,

		[Description("Folders")]
		FOLDERS = 2
	}
}