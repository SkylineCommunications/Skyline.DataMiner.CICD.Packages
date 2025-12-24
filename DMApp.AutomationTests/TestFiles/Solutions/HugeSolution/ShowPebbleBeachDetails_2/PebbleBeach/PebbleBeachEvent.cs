namespace ShowPebbleBeachDetails_2.PebbleBeach
{
	using System;

	public class PebbleBeachEvent
	{
		public string Id { get; set; }

		public string Uid { get; set; }

		public string PlaylistId { get; set; }

		public string Title { get; set; }

		public string HouseId { get; set; }

		public string Status { get; set; }

		public string Type { get; set; }

		public DateTime Start { get; set; }

		public TimeSpan Duration { get; set; }

		public string ReconcileKey { get; set; }

		public string Source { get; set; }

		public string BackupSource { get; set; }

		public string Destination { get; set; }

		public string BackupDestination { get; set; }

		public string BlockId { get; set; }

		public string RunningState { get; set; }

		public string Response { get; set; }

		public string PossibleSources { get; set; }
	}
}