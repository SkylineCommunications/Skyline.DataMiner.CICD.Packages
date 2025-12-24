namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleNumeric : Numeric, IYleInteractiveWidget
	{
		public YleNumeric(double value) : base(value)
		{
			FocusLost += YleNumeric_FocusLost;
		}

		public Guid Id { get; set; } = Guid.Empty;

		public string Name { get; set; } = string.Empty;

		public Helpers Helpers { get; set; }

		public new object Value
		{
			get => base.Value;
			set => base.Value = Convert.ToDouble(value);
		}

		public new event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		private void YleNumeric_FocusLost(object sender, NumericFocusLostEventArgs e)
		{
			Helpers?.Log(nameof(YleNumeric), nameof(YleNumeric_FocusLost), $"USER INPUT: user changed value to {e.Value}. Numeric Name='{Name}'. ID='{Id}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.Value));
		}
	}
}