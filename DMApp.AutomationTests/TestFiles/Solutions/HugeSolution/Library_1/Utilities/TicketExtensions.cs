namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;

	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Net.Ticketing.Validators;

	public static class TicketExtension
	{
		public static int GetIntegerFieldValue(this Ticket ticket, string fieldName)
		{
			// When the ticket is assigned to a user, the type of the value object in the custom fields dictionary for a dropdown list
			// is Skyline.DataMiner.Net.Ticketing.Validators.GenericEnumEntry`1[System.Int32]
			// else, the value is System.Int64
			if (!ticket.CustomTicketFields.TryGetValue(fieldName, out object value))
			{
				throw new TicketFieldDoesNotExistException(fieldName);
			}

			GenericEnumEntry<int> enumValue = value as GenericEnumEntry<int>;
			if (enumValue != null)
			{
				return enumValue.Value;
			}
			else
			{
				return Convert.ToInt32(value);
			}
		}
	}
}