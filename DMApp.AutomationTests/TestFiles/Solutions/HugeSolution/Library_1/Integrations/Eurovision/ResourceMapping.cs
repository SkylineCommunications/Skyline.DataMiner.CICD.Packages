namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public static class ResourceMapping
	{
		/// <summary>
		/// Maps the Satellite names as received in the EBU synopsis to the Satellite names in DataMiner.
		/// </summary>
		public static readonly IReadOnlyDictionary<string, string> Satellites = new Dictionary<string, string>
		                                                                        {
			                                                                        { "ABS-3A", "ABS-3" },
			                                                                        { "ASTRA 3B", "Astra 3B" },
			                                                                        { "EU10A", "Eutelsat 10A (E10A)" },
			                                                                        { "EU16A", "Eutelsat 16A (E16A)" },
			                                                                        { "EU36B", "Eutelsat 36A (E36A)" },
			                                                                        { "EU5 WA", "Eutelsat 5A (E5WA)" },
			                                                                        { "Eutelsat 7B", "Eutelsat 7B (E7A)" },
			                                                                        { "IS-905", "Intelsat 905" }
		                                                                        };
	}
}