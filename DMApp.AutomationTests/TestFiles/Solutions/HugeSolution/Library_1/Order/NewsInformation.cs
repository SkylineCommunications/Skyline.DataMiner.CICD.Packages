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

    public class NewsInformation : DisplayedObject, IYleChangeTracking, ICloneable
    {
        private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();

        private string newsCameraOperator;
        private string journalist;
        private string virveCommandGroupOne;
        private string virveCommandGroupTwo;
        private string additionalInformation;

        public event EventHandler<string> NewsCameraOperatorChanged;

		public NewsInformation()
		{

		}

		private NewsInformation(NewsInformation other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        [ChangeTracked]
        public string NewsCameraOperator
        {
            get => newsCameraOperator;
            set
            {
                newsCameraOperator = value;
                NewsCameraOperatorChanged?.Invoke(this, newsCameraOperator);
            }
        }

        public event EventHandler<string> JournalistChanged;

        [ChangeTracked]
        public string Journalist
        {
            get => journalist;
            set
            {
                journalist = value;
                JournalistChanged?.Invoke(this, journalist);
            }
        }

        public event EventHandler<string> VirveCommandGroupOneChanged;

        [ChangeTracked]
        public string VirveCommandGroupOne
        {
            get => virveCommandGroupOne;
            set
            {
                virveCommandGroupOne = value;
                VirveCommandGroupOneChanged?.Invoke(this, virveCommandGroupOne);
            }
        }

        public event EventHandler<string> VirveCommandGroupTwoChanged;

        [ChangeTracked]
        public string VirveCommandGroupTwo
        {
            get => virveCommandGroupTwo;
            set
            {
                virveCommandGroupTwo = value;
                VirveCommandGroupTwoChanged?.Invoke(this, virveCommandGroupTwo);
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
        public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ClassChange(nameof(NewsInformation))) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

        [JsonIgnore]
        public string UniqueIdentifier => nameof(NewsInformation);

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
            if (!(oldObjectInstance is NewsInformation oldNewsInformation)) throw new ArgumentException($"Argument is not of type {nameof(NewsInformation)}", nameof(oldObjectInstance));

            return ChangeTrackingHelper.GetChangeComparedTo(this, oldNewsInformation, new ClassChange(nameof(NewsInformation)), helpers);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NewsInformation other)) return false;

            bool equal = true;
            foreach (var property in typeof(NewsInformation).GetProperties())
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

				hash = hash * 23 + NewsCameraOperator.GetHashCode();
				hash = hash * 23 + Journalist.GetHashCode();
				hash = hash * 23 + VirveCommandGroupOne.GetHashCode();
				hash = hash * 23 + VirveCommandGroupTwo.GetHashCode();
				hash = hash * 23 + AdditionalInformation.GetHashCode();
				return hash;
			}
		}

		public object Clone()
		{
			return new NewsInformation(this);
		}
	}
}