namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class SportsPlanning : DisplayedObject, IYleChangeTracking, ICloneable
	{
		private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();

		private string sport;
		private string description;
		private string commentary;
		private string commentary2;
		private double competitionTime; // ms since 01/01/1970
		private string journalist1;
		private string journalist2;
		private string journalist3;
		private string location;
		private string technicalResources;
		private string liveHighlightsFile;
		private double requestedBroadcastTime; // ms since 01/01/1970
		private string productionNumberPlasmaId;
		private string productNumberCeiton;
		private string costDepartment;
		private string additionalInformation;

		private SportsPlanning(SportsPlanning other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public SportsPlanning()
        {
			// Init Competition Time and Requested Broadcast Time
			double value = DateTime.Now.AddDays(1).ConvertToCustomDatetimePropertyForReservation();
			competitionTime = value;
			requestedBroadcastTime = value;
        }

		public event EventHandler<string> SportChanged;

		[ChangeTracked]
		public string Sport
		{
			get => sport;
			set
            {
				sport = value;
				SportChanged?.Invoke(this, sport);
            }
		}

		public event EventHandler<string> DescriptionChanged;

		[ChangeTracked]
		public string Description
		{
			get => description;
			set
			{
				description = value;
				DescriptionChanged?.Invoke(this, description);
			}
		}

		public event EventHandler<string> CommentaryChanged;

		[ChangeTracked]
		public string Commentary
		{
			get => commentary;
			set
			{
				commentary = value;
				CommentaryChanged?.Invoke(this, commentary);
			}
		}

		public event EventHandler<string> Commentary2Changed;

		[ChangeTracked]
		public string Commentary2
		{
			get => commentary2;
			set
			{
				commentary2 = value;
				Commentary2Changed?.Invoke(this, commentary2);
			}
		}

		public event EventHandler<double> CompetitionTimeChanged;

		/// <summary>
		/// Milliseconds since 01/01/1970
		/// </summary>
		[ChangeTracked]
		public double CompetitionTime
		{
			get => competitionTime;
			set
			{
				competitionTime = value;
				CompetitionTimeChanged?.Invoke(this, competitionTime);
			}
		}

		public event EventHandler<string> JournalistOneChanged;

		[ChangeTracked]
		public string JournalistOne
		{
			get => journalist1;
			set
			{
				journalist1 = value;
				JournalistOneChanged?.Invoke(this, journalist1);
			}
		}

		public event EventHandler<string> JournalistTwoChanged;

		[ChangeTracked]
		public string JournalistTwo
		{
			get => journalist2;
			set
			{
				journalist2 = value;
				JournalistTwoChanged?.Invoke(this, journalist2);
			}
		}

		public event EventHandler<string> JournalistThreeChanged;

		[ChangeTracked]
		public string JournalistThree
		{
			get => journalist3;
			set
			{
				journalist3 = value;
				JournalistThreeChanged?.Invoke(this, journalist3);
			}
		}

		public event EventHandler<string> LocationChanged;

		[ChangeTracked]
		public string Location
		{
			get => location;
			set
			{
				location = value;
				LocationChanged?.Invoke(this, location);
			}
		}

		public event EventHandler<string> TechnicalResourcesChanged;

		[ChangeTracked]
		public string TechnicalResources
		{
			get => technicalResources;
			set
			{
				technicalResources = value;
				TechnicalResourcesChanged?.Invoke(this, technicalResources);
			}
		}

		public event EventHandler<string> LiveHighlightsFileChanged;

		[ChangeTracked]
		public string LiveHighlightsFile
		{
			get => liveHighlightsFile;
			set
			{
				liveHighlightsFile = value;
				LiveHighlightsFileChanged?.Invoke(this, liveHighlightsFile);
			}
		}

		public event EventHandler<double> RequestedBroadcastTimeChanged;

		/// <summary>
		/// Milliseconds since 01/01/1970
		/// </summary>
		[ChangeTracked]
		public double RequestedBroadcastTime
		{
			get => requestedBroadcastTime;
			set
			{
				requestedBroadcastTime = value;
				RequestedBroadcastTimeChanged?.Invoke(this, requestedBroadcastTime);
			}
		}

		public event EventHandler<string> ProductionNumberPlasmaIdChanged;

		[ChangeTracked]
		public string ProductionNumberPlasmaId
		{
			get => productionNumberPlasmaId;
			set
			{
				productionNumberPlasmaId = value;
				ProductionNumberPlasmaIdChanged?.Invoke(this, productionNumberPlasmaId);
			}
		}

		public event EventHandler<string> ProductNumberCeitonChanged;

		[ChangeTracked]
		public string ProductNumberCeiton
		{
			get => productNumberCeiton;
			set
			{
				productNumberCeiton = value;
				ProductNumberCeitonChanged?.Invoke(this, productNumberCeiton);
			}
		}

		public event EventHandler<string> CostDepartmentChanged;

		[ChangeTracked]
		public string CostDepartment
		{
			get => costDepartment;
			set
			{
				costDepartment = value;
				CostDepartmentChanged?.Invoke(this, costDepartment);
			}
		}

		public event EventHandler<string> AdditionalInformationChanged;

		[ChangeTracked]
		public string AdditionalInformation
		{
			get => additionalInformation;
			set
			{
				additionalInformation = value;
				AdditionalInformationChanged?.Invoke(this, additionalInformation);
			}
		}

		/// <summary>
		/// Gets a boolean indicating if Change Tracking has been enabled for this object.
		/// </summary>
		[JsonIgnore]
        public bool ChangeTrackingStarted { get; private set; }

		[JsonIgnore]
		public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ClassChange(nameof(SportsPlanning))) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

		[JsonIgnore]
		public string UniqueIdentifier => nameof(SportsPlanning);

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		/// <summary>
		/// Resets Change Tracking.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		public void AcceptChanges(Helpers helpers = null)
		{
			ChangeTrackingStarted = true;
			ChangeTrackingHelper.AcceptChanges(this, initialPropertyValues, helpers);
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is SportsPlanning oldSportsPlanning)) throw new ArgumentException($"Argument is not of type {nameof(SportsPlanning)}", nameof(oldObjectInstance));

			return ChangeTrackingHelper.GetChangeComparedTo(this, oldSportsPlanning, new ClassChange(nameof(SportsPlanning)), helpers);
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SportsPlanning other)) return false;

			bool equal = true;
			foreach (var property in typeof(SportsPlanning).GetProperties())
			{
				equal &= property.GetValue(this).Equals(property.GetValue(other));
			}

			return equal;
		}

        public override int GetHashCode()
        {
	        unchecked
	        {
		        int hash = 17;

		        hash = hash * 23 + Sport.GetHashCode();
		        hash = hash * 23 + Description.GetHashCode();
		        hash = hash * 23 + Commentary.GetHashCode();
		        hash = hash * 23 + Commentary2.GetHashCode();
		        hash = hash * 23 + CompetitionTime.GetHashCode();
		        hash = hash * 23 + JournalistOne.GetHashCode();
		        hash = hash * 23 + JournalistTwo.GetHashCode();
		        hash = hash * 23 + JournalistThree.GetHashCode();
		        hash = hash * 23 + Location.GetHashCode();
		        hash = hash * 23 + TechnicalResources.GetHashCode();
		        return hash;
	        }
        }

		public object Clone()
		{
			return new SportsPlanning(this);
		}
	}
}