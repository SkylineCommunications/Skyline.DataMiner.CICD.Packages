namespace Library.UI.Filters
{
	using System;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class GuidFilterSection<T> : StringFilterSection<T>, IFilter<T>
	{
		public GuidFilterSection(string filterName, Func<object, FilterElement<T>> emptyFilter) : base(filterName, emptyFilter)
		{
			
		}

		public override bool IsValid => Guid.TryParse(filterContentTextBox.Text, out var guid);

		public override object Value => Guid.Parse(filterContentTextBox.Text);
	}
}
