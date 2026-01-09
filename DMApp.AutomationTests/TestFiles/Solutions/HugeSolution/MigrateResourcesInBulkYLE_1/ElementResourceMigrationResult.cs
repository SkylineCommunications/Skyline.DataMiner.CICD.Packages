namespace MigrateResourcesInBulkYLE_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;

	[Serializable]
	public class ElementResourceMigrationResult
	{
		public ElementResourceMigrationResult()
		{

		}

		public ElementResourceMigrationResult(string oldElementName, string newElementName)
		{
			OldElementName = oldElementName;
			NewElementName = newElementName;
		}

		[JsonProperty]
		public string OldElementName { get; private set; }

		[JsonProperty]
		public string NewElementName { get; private set; }

		[JsonProperty]
		public List<ResourceMigrationResult> ResourceMigrationResults { get; private set; } = new List<ResourceMigrationResult>();

		[JsonProperty]
		public bool IsSuccessful { get; private set; } = true;

		[JsonProperty]
		public string ErrorMessage { get; private set; }

		public void SetErrorMessage(string message)
		{
			IsSuccessful = false;
			ErrorMessage = message;
		}

		public void AddResourceMigrationResults(IEnumerable<ResourceMigrationResult> results)
		{
			if (results == null) return;

			ResourceMigrationResults.AddRange(results);

			if (results.Any(r => !r.IsSuccessful)) SetErrorMessage("Not all resource migrations succeeded");
		}
	}
}
