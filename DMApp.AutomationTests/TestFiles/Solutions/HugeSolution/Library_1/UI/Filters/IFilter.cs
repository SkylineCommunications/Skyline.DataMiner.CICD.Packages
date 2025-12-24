using Skyline.DataMiner.Net.Messages.SLDataGateway;

namespace Library.UI.Filters
{
	public interface IFilter<T>
	{
		bool IsActive { get; }

		bool IsValid { get; }

		object Value { get; }

		FilterElement<T> Filter { get; }
	}
}
