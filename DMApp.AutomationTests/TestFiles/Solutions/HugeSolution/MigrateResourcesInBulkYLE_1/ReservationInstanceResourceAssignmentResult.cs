namespace MigrateResourcesInBulkYLE_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;

	[Serializable]
	public class ReservationInstanceResourceAssignmentResult
	{
		public ReservationInstanceResourceAssignmentResult()
		{

		}

		public ReservationInstanceResourceAssignmentResult(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; set; }

		[JsonProperty]
		public bool IsSuccessful { get; private set; } = true;

		[JsonProperty]
		public string ErrorMessage { get; private set; }

		public void SetErrorMessage(string message)
		{
			IsSuccessful = false;
			ErrorMessage = message;
		}
	}
}
