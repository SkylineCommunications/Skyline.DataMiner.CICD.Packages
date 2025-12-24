namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Filters;

	public abstract class MethodCallIdentifier
	{
		[JsonProperty(Order = -4)]
		public string ClassName { get; set; }

		[JsonProperty(Order = -3)]
		public string MethodName { get; set; }

		[JsonProperty(Order = -2)]
		public string ObjectName { get; set; }

		public bool MatchesFilter(MethodCallFilter filter)
		{
			bool matchesFilter = true;

			matchesFilter &= filter.ClassNames == null 
			                 || (filter.ClassNameFilterType == MethodCallFilter.FilterType.NameEqualsOneOrMore && filter.ClassNames.Contains(ClassName))
			                 || (filter.ClassNameFilterType == MethodCallFilter.FilterType.NameDoesNotEqual && !filter.ClassNames.Contains(ClassName));

			matchesFilter &= filter.MethodNames == null 
			                 || (filter.MethodNameFilterType == MethodCallFilter.FilterType.NameEqualsOneOrMore && filter.MethodNames.Contains(MethodName))
			                 || (filter.MethodNameFilterType == MethodCallFilter.FilterType.NameDoesNotEqual && !filter.MethodNames.Contains(MethodName));

			matchesFilter &= filter.ObjectNames == null 
			                 || (filter.ObjectNameFilterType == MethodCallFilter.FilterType.NameEqualsOneOrMore && filter.ObjectNames.Contains(ObjectName))
			                 || (filter.ObjectNameFilterType == MethodCallFilter.FilterType.NameDoesNotEqual && !filter.ObjectNames.Contains(ObjectName));

			return matchesFilter;
		}

		public bool IsSameMethodAs(MethodCallIdentifier second)
		{
			if (second == null) return false;

			return ClassName == second.ClassName && MethodName == second.MethodName;
		}

		public MethodCallIdentifier GetMethodCallIdentifier()
		{
			return this;
		}

		protected void SetMethodCallIdentifier(MethodCallIdentifier identifier)
		{
			if (identifier is null) throw new ArgumentNullException(nameof(identifier));
			
			ClassName = identifier.ClassName;
			MethodName = identifier.MethodName;
		}
	}
}
