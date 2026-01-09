namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
	using System.Text.RegularExpressions;
	using Library_1.Utilities;

	public class Discreet : ICloneable
	{
		private readonly string discreet;

		public Discreet(string discreet, object orderByValue = null)
		{
			this.discreet = discreet;
			OrderByValue = orderByValue ?? discreet;
			InitializeLinkedParameterValues();
		}

		private Discreet(Discreet other)
		{
			this.discreet = other.discreet;
			CloneHelper.CloneProperties(other, this);
		}

		private void InitializeLinkedParameterValues()
		{
			if (discreet == null) return;
			if (discreet.Contains("[") && discreet.Contains(":") && discreet.Contains("]"))
			{
				Match discreetNameMatch = Regex.Match(discreet, @".*\[");
				Match parameterNameMatch = Regex.Match(discreet, @"\[.*:");
				Match parameterValueMatch = Regex.Match(discreet, @":.*\]");

				DisplayValue = discreetNameMatch.Value.Trim(' ', '[');
				LinkedParentName = parameterNameMatch.Value.Trim('[', ':');
				LinkedParentValue = parameterValueMatch.Value.Trim(':', ']').ToUpper();
			}
			else
			{
				DisplayValue = discreet;
			}
		}

		/// <summary>
		/// The value of the discreet as it should be displayed in the UI.
		/// In case of linked parameters this will be the first piece of the discreet.
		/// In case of a non-linked parameter this will be the same as the internal value of the discreet.
		/// </summary>
		public string DisplayValue { get; private set; }

		/// <summary>
		/// Value of the Discreet as used internally. For linked discreets, this will be "DisplayValue[Linked Parent Name:Linked Parent Value]"
		/// </summary>
		public string InternalValue
		{
			get
			{
				return discreet;
			}
		}

		public object OrderByValue { get; }

		public string LinkedParentName { get; private set; }

		/// <summary>
		/// The selected value of the linked parent discreet to which this discreet applies.
		/// This value is in uppercase.
		/// </summary>
		public string LinkedParentValue { get; private set; }

		public override string ToString()
		{
			return discreet;
		}

		public override bool Equals(object obj)
		{
			Discreet other = obj as Discreet;
			if (other == null) return false;

			return discreet.Equals(other.discreet);
		}

		public override int GetHashCode()
		{
			return discreet.GetHashCode();
		}

		public object Clone()
		{
			return new Discreet(this);
		}
	}
}