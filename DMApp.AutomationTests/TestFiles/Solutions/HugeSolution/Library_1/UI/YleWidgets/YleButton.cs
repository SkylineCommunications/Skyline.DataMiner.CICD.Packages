namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleButton : Button, IYleInteractiveWidget
	{
		public YleButton(string text) : base(text)
		{
			base.Pressed += YleButton_Pressed;
			Name = text;
		}

		public Guid Id { get; set; } = Guid.Empty;

		public string Name { get; set; }

		public Helpers Helpers { get; set; }

		public object Value { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		public new event EventHandler<YleValueWidgetChangedEventArgs> Pressed;

		private void YleButton_Pressed(object sender, EventArgs e)
		{
			Helpers?.Log(nameof(YleButton), nameof(YleButton_Pressed), $"USER INPUT: user pressed '{Text}' button. Button Name='{Name}'. ID='{Id}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e));
			Pressed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e));
		}
	}
}