using System;
using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class IsilonBuExport
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public IsilonBuExport()
		{
			BackupOrigin = default(BackupOrigins?);
			ProductionDepartmentName = default(string);
			ProgramName = default(string);
			VsaEpisodeName = default(string);
			BackupStart = default(DateTime);
			BackupEnd = default(DateTime);
			AdditionalInformation = default(string);
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public BackupOrigins? BackupOrigin { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProductionDepartmentName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProgramName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VsaEpisodeName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime BackupStart { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime BackupEnd { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AdditionalInformation { get; set; }
	}
}