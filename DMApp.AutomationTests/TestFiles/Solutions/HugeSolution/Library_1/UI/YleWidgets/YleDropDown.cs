namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleDropDown : DropDown, IYleInteractiveWidget
	{
		public YleDropDown()
		{
			base.Changed += YleDropDown_Changed;
		}

		public YleDropDown(IEnumerable<string> options, string selected = null) : base(options, selected)
		{
			base.Changed += YleDropDown_Changed;
		}

		public Guid Id { get; set; } = Guid.Empty;

		public string Name { get; set; } = String.Empty;

		public Helpers Helpers { get; set; }

		public object Value
		{
			get => base.Selected;
			set => base.Selected = Convert.ToString(value);
		}

		public new event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		private string Identifier => $"{nameof(YleDropDown)} {nameof(Name)}='{Name}' {nameof(Id)}='{Id}'";

		private void YleDropDown_Changed(object sender, DropDownChangedEventArgs e)
		{
			Log(nameof(YleDropDown_Changed), $"USER INPUT: user changed value from '{e.Previous}' to '{e.Selected}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.Selected, e.Previous));
		}

		private void Log(string nameOfMethod, string message)
		{
			Helpers?.Log(nameof(YleDropDown), nameOfMethod, $"{message}. {Identifier}");
		}
	}
}