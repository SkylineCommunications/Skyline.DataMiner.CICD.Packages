namespace MigrateResources_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	[Serializable]
	public class ResourceMigrationResult
	{
		private FunctionResource oldResource;
		private FunctionResource newResource;

		public ResourceMigrationResult()
		{

		}

		public ResourceMigrationResult(FunctionResource resource)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			oldResource = resource;

			Name = oldResource.Name;
			OldId = oldResource.ID;
		}

		[JsonIgnore]
		public FunctionResource OldResource => oldResource;

		[JsonIgnore]
		public FunctionResource NewResource
		{
			get
			{
				return newResource;
			}

			set
			{
				newResource = value;

				NewId = newResource?.ID;
			}
		}

		[JsonProperty]
		public string Name { get; private set; }

		[JsonProperty]
		public Guid OldId { get; private set; }

		[JsonProperty]
		public Guid? NewId { get; private set; }

		[JsonProperty]
		public bool IsSuccessful { get; private set; } = true;

		[JsonProperty]
		public string ErrorMessage { get; private set; }

		[JsonProperty]
		public List<ReservationInstanceResourceAssignmentResult> ReservationInstanceResourceAssignmentResults { get; private set; }

		public void SetErrorMessage(string message)
		{
			IsSuccessful = false;
			ErrorMessage = message;
		}

		public void SetReservationInstanceResourceAssignmentResults(List<ReservationInstanceResourceAssignmentResult> results)
		{
			ReservationInstanceResourceAssignmentResults = results;

			if (results.Any(r => !r.IsSuccessful)) SetErrorMessage("Not all reservation instance resource assignments succeeded");
		}
	}
}