namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;

	public class UpdateResult
	{
		public UpdateResult()
        {

        }

		public UpdateResult(IEnumerable<UpdateResult> results)
        {
			UpdateWasSuccessful = true;
			foreach (var result in results)
			{
				UpdateWasSuccessful &= result.UpdateWasSuccessful;
				Tasks.AddRange(result.Tasks);
				Exceptions.AddRange(result.Exceptions);
			}
		}

		public bool UpdateWasSuccessful { get; set; }

		public List<Task> Tasks { get; set; } = new List<Task>();

		public List<Exception> Exceptions { get; set; } = new List<Exception>();

		public TimeSpan Duration { get; set; }

		public void Add(UpdateResult updateResultToAdd)
		{
			Tasks.AddRange(updateResultToAdd.Tasks);
			Exceptions.AddRange(updateResultToAdd.Exceptions);
			UpdateWasSuccessful &= updateResultToAdd.UpdateWasSuccessful;
		}
	}
}
