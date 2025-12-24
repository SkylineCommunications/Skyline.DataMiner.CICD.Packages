namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public interface IYleInteractiveWidget : IYleWidget
	{
		object Value { get; set; }

		bool IsVisible { get; set; }

		bool IsEnabled { get; set; }

		event EventHandler<YleValueWidgetChangedEventArgs> Changed;
	}

	public class YleValueWidgetChangedEventArgs
	{
		public YleValueWidgetChangedEventArgs(Guid id, object value, object previousValue = null)
		{
			Id = id;
			Value = value;
			PreviousValue = previousValue;
		}

		public Guid Id { get; private set; }

		public object Value { get; private set; }

		public object PreviousValue { get; private set; }
	}
}