namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Net.Profiles;

	/// <summary>
	/// This class represents an SRM ProfileParameter.
	/// </summary>
	public class ProfileParameter : IYleChangeTracking, IEqualityComparer<ProfileParameter>, ICloneable
	{
		private object value;
		private object initialValue;
		private List<Discreet> discreets = new List<Discreet>();
		private ValidationInfo valueValidation;

		public ProfileParameter()
		{
		}

		public ProfileParameter(Net.Profiles.Parameter netParameter)
		{
			if (netParameter == null) throw new ArgumentNullException(nameof(netParameter));

			Name = netParameter.Name;
			Id = netParameter.ID;
			Category = netParameter.Categories;
			Type = (ParameterType)netParameter.Type;
			Discreets = netParameter.Type == Net.Profiles.Parameter.ParameterType.Discrete ? netParameter.Discretes.Select(x => new Discreet(x, ConvertDiscreetToOrderByValue(x))).ToList() : new List<Discreet>();
			DefaultValue = netParameter.DefaultValue;
			Stepsize = netParameter.Stepsize;
			Decimals = netParameter.Decimals;
			RangeMax = netParameter.RangeMax;
			RangeMin = netParameter.RangeMin;
			Unit = netParameter.Units;
			
			switch (Type)
            {
				case ParameterType.Text:
				case ParameterType.Discrete:
					Value = (DefaultValue != null) ? DefaultValue.StringValue : String.Empty;
					break;
				case ParameterType.Number:
					Value = (DefaultValue != null) ? DefaultValue.DoubleValue : RangeMin;
					break;
				default:
					// Unsupported type
					break;
            }
            
			AcceptChanges();		
		}

		private ProfileParameter(ProfileParameter other)
		{
			CloneHelper.CloneProperties(other, this);

			Discreets = other.Discreets.Select(d => d.Clone()).Cast<Discreet>().ToList();
			Value = other.Value;
			DefaultValue = other.DefaultValue;
		}

		/// <summary>
		/// The id of the profile parameter
		/// Only required when action is "NEW" or "EDIT"
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the name of the ProfileParameter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Property set by controller and used by UI for validation.
		/// </summary>
		[JsonIgnore]
		public ValidationInfo ValueValidation
		{
			get
			{
				if (valueValidation == null) valueValidation = new ValidationInfo();
				return valueValidation;
			}

			set
			{
				valueValidation = value;
			}
		}

		/// <summary>
		/// The value of the profile parameter
		/// </summary>
		[ChangeTracked]
		public object Value
		{
			get => value;
			set
			{
				if (this.value != null && this.value.Equals(value)) return;

				this.value = value;
				ValueChanged?.Invoke(this, this.value);
			}
		}

		/// <summary>
		/// Gets the string representation of the value contained in this ProfileParameter.
		/// </summary>
		[JsonIgnore]
		public string StringValue
		{
			get => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// This event is called when the value of this ProfileParameter is updated.
		/// </summary>
		public event EventHandler<object> ValueChanged;

		/// <summary>
		/// The categories of the Profile Parameter.
		/// </summary>
		public ProfileParameterCategory Category { get; set; }

		/// <summary>
		/// Indicates if the Profile Parameter is a Capability.
		/// </summary>
		public bool IsCapability
		{
			get => (Category & ProfileParameterCategory.Capability) == ProfileParameterCategory.Capability;
		}

		/// <summary>
		/// Gets a boolean indicating if Change Tracking has been enabled for this object.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		[JsonIgnore]
		public bool ChangeTrackingStarted { get; private set; }

		public bool IsNonInterfaceDtrParameter => Name.StartsWith("_");

		public List<Discreet> Discreets
		{
			get => discreets;
			set => discreets = value ?? new List<Discreet>();
		}

		public ParameterType Type { get; set; }

		public ParameterValue DefaultValue { get; set; }

		public double RangeMin { get; set; }

		public double RangeMax { get; set; }

		public double Stepsize { get; set; }

		public int Decimals { get; set; }

		public string Unit { get; set; }

		public override bool Equals(object obj)
		{
			ProfileParameter other = obj as ProfileParameter;
			if (other == null) return false;

			return Id.Equals(other.Id);
		}

		public bool Equals(ProfileParameter x, ProfileParameter y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(ProfileParameter obj)
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Resets Change Tracking.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		public void AcceptChanges(Helpers helpers = null)
		{
			ChangeTrackingStarted = true;
			initialValue = Value;
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is ProfileParameter oldProfileParameter)) throw new ArgumentException($"Argument is not of type {nameof(ProfileParameter)}", nameof(oldObjectInstance));

			return new ProfileParameterChange(this, oldProfileParameter.Value, Value);
		}

		[JsonIgnore]
		public Change Change => ChangeTrackingStarted ? new ProfileParameterChange(this, initialValue, Value) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

		[JsonIgnore]
		public string UniqueIdentifier => Name;

		[JsonIgnore]
		public string DisplayName => UniqueIdentifier;

		public override string ToString()
        {
            return $"{Name} = {StringValue}";
        }

		public Library.Solutions.SRM.Model.Parameter GetParameterForBooking()
		{
			return new Library.Solutions.SRM.Model.Parameter
			{
				Id = Id,
				Value = Convert.ToString(Value), // VSC: use current culture to convert!
			};
		}

		public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

		public object Clone()
		{
			return new ProfileParameter(this);
		}

		private object ConvertDiscreetToOrderByValue(string discreetValue)
		{
			if (Id == ProfileParameterGuids.RollOff)
			{
				return int.Parse(discreetValue.Trim('%'));
			}

			return null;
		}
	}
}