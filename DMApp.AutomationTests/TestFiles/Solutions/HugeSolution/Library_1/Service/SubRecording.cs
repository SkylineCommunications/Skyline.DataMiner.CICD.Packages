namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// A sub recording for a specific recording.
	/// </summary>
	public class SubRecording : DisplayedObject, IYleChangeTracking, ICloneable
	{
		private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();
		
		public SubRecording()
		{
			Id = Guid.NewGuid();
			EstimatedTimeSlotStart = DateTime.Now.AddDays(1);
			EstimatedTimeSlotEnd = DateTime.Now.AddDays(1);

			AcceptChanges(null);
		}

		private SubRecording(SubRecording other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		/// <summary>
		/// Used to identify the object by a controller.
		/// </summary>
		[JsonIgnore]
		public Guid Id { get; }

		/// <summary>
		/// The name of this sub recording.
		/// </summary>
		[ChangeTracked]
		public string Name { get; set; }

		/// <summary>
		/// Additional information about this sub recording.
		/// </summary>
		[ChangeTracked]
		public string AdditionalInformation { get; set; }

		/// <summary>
		/// The start of the estimated time slot for this sub recording.
		/// </summary>
		[ChangeTracked]
		public DateTime EstimatedTimeSlotStart { get; set; }

		/// <summary>
		/// The end of the estimated time slot for this sub recording.
		/// </summary>
		[ChangeTracked]
		public DateTime EstimatedTimeSlotEnd { get; set; }

		/// <summary>
		/// The description for this time slot.
		/// </summary>
		[ChangeTracked]
		public string TimeslotDescription { get; set; }

		[JsonIgnore]
		public bool ChangeTrackingStarted { get; private set; }

		[JsonIgnore]
		public string UniqueIdentifier => Name;

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		[JsonIgnore]
		public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ClassChange(nameof(SubRecording))) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

		public void AcceptChanges(Helpers helpers = null)
		{
			ChangeTrackingStarted = true;
			ChangeTrackingHelper.AcceptChanges(this, initialPropertyValues, helpers);
		}

		public object Clone()
		{
			return new SubRecording(this);
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is SubRecording oldSubRecording)) throw new ArgumentException($"Argument is not of type {nameof(SubRecording)}", nameof(oldObjectInstance));

			return ChangeTrackingHelper.GetChangeComparedTo(this, oldSubRecording, new ClassChange(nameof(SubRecording)));
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Sub recording name: " + Name + "|");
            sb.AppendLine($"Sub recording start: " + Convert.ToString(EstimatedTimeSlotStart, CultureInfo.InvariantCulture) + "|");
            sb.AppendLine($"Sub recording end: " + Convert.ToString(EstimatedTimeSlotEnd, CultureInfo.InvariantCulture) + "|");
            sb.AppendLine($"Sub recording time slot description: " + TimeslotDescription);

            return sb.ToString();
        }
	}
}