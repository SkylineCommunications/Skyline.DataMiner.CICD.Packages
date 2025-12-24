namespace MigrateResources_1
{
	using System;
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