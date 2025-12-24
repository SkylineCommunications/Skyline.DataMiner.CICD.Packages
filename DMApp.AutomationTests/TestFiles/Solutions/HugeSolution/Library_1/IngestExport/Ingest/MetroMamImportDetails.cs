namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class MetroMamImportDetails : MaterialIngestDetails
	{
		private const MaterialTypes materialType = MaterialTypes.MetroMam;

		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public MetroMamImportDetails()
		{
			ProgramName = default(string);
			Id = default(string);
			AdditionalInformation = default(string);
		}

		[JsonProperty]
		public override string Type
		{
			get
			{
				return EnumExtensions.GetDescriptionFromEnumValue(materialType);
			}
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProgramName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

        public override string ToString()
        {
			var sb = new StringBuilder();

			sb.AppendLine($"Program name: {ProgramName} | ");
			sb.AppendLine($"ID: {Id} | ");
			sb.AppendLine($"Additional information: {AdditionalInformation} | ");

			return sb.ToString();
		}
    }
}
