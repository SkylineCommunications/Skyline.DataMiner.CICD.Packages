using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Net.Messages.SLDataGateway;

namespace Library.UI.Filters
{
	public static class FilterHelper
	{
		public static IEnumerable<IFilter<TToBeFiltered>> GetIndividualFilters<TToBeFiltered>(this Section section)
		{
			var fieldValues = section.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Select(field => field.GetValue(section)).ToList();

			var fieldsImplementingInterface = fieldValues.OfType<IFilter<TToBeFiltered>>().ToList();

			var fieldsContainingCollectionOfInterface = fieldValues.OfType<IEnumerable<IFilter<TToBeFiltered>>>().SelectMany(collection => collection).ToList();

			var filters = fieldsImplementingInterface.Concat(fieldsContainingCollectionOfInterface).ToList();

			return filters;
		}

		public static bool ActiveFiltersAreValid<TToBeFiltered>(this Section section)
		{
			var individualFilters = GetIndividualFilters<TToBeFiltered>(section);

			if (!individualFilters.Any()) return true;

			return individualFilters.Where(filter => filter.IsActive).All(filter => filter.IsValid);
		}

		public static bool TryGetCombinedFilterElement<TToBeFiltered>(this Section section, out ANDFilterElement<TToBeFiltered> filter)
		{
			try
			{
				filter = GetCombinedFilterElement<TToBeFiltered>(section);
				return true;
			}
			catch (Exception)
			{
				filter = null;
				return false;
			}
		}

		public static ANDFilterElement<TToBeFiltered> GetCombinedFilterElement<TToBeFiltered>(this Section section, params FilterElement<TToBeFiltered>[] defaultFilters)
		{
			List<FilterElement<TToBeFiltered>> individualActiveFilterElements = new List<FilterElement<TToBeFiltered>>();

			individualActiveFilterElements.AddRange(defaultFilters);
			individualActiveFilterElements.AddRange(GetIndividualFilters<TToBeFiltered>(section).Where(filter => filter.IsActive).Select(filter => filter.Filter));

			if (!individualActiveFilterElements.Any()) throw new InvalidOperationException("Unable to find any active filters");

			return new ANDFilterElement<TToBeFiltered>(individualActiveFilterElements.ToArray());
		}
	}
}
