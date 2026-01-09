namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleCheckBox : CheckBox, IYleInteractiveWidget
	{
		public YleCheckBox(string text) : base(text)
		{
			base.Changed += YleCheckBox_Changed;
			Name = text;
		}

		public Guid Id { get; set; } = Guid.Empty;

		public string Name { get; set; }

		public Helpers Helpers { get; set; }

		public object Value
		{
			get => IsChecked;
			set => IsChecked = Convert.ToBoolean(value);
		}

		public new event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		private void YleCheckBox_Changed(object sender, CheckBoxChangedEventArgs e)
		{
			Helpers?.Log(nameof(YleCheckBox), nameof(YleCheckBox_Changed), $"USER INPUT: user {(IsChecked ? "checked" : "unchecked")} '{Text}' checkbox. CheckBox Name='{Name}'. ID='{Id}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.IsChecked));
		}
	}
}