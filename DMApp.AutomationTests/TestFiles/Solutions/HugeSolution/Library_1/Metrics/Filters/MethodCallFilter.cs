namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Net.Time;

	public abstract class MethodCallFilter
	{
		public enum FilterType
		{
			NameEqualsOneOrMore,
			NameDoesNotEqual
		}

		public List<string> ClassNames { get; set; }

		public FilterType ClassNameFilterType { get; set; } = FilterType.NameEqualsOneOrMore;

		public List<string> MethodNames { get; set; }

		public FilterType MethodNameFilterType { get; set; } = FilterType.NameEqualsOneOrMore;

		public List<string> ObjectNames { get; set; }

		public FilterType ObjectNameFilterType { get; set; } = FilterType.NameEqualsOneOrMore;

		public TimeRange TimeRange { get; set; }
	}
}
