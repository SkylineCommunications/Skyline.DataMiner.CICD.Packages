namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleTextBox : TextBox, IYleInteractiveWidget
	{
		public YleTextBox(string text) : base(text)
		{
			FocusLost += YleTextBox_FocusLost;
		}

		public YleTextBox() : this(string.Empty)
		{
		}

		public Guid Id { get; set;  } = Guid.Empty;

		public string Name { get; set; } = string.Empty;
		
		public Helpers Helpers { get; set; }

		public object Value
		{
			get => Text;
			set => Text = Convert.ToString(value);
		}

		public new event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		public Predicate<string> ValidationPredicate { get; set; } = content => true;

		public bool IsValid => TextIsValid();

		private void YleTextBox_FocusLost(object sender, TextBoxFocusLostEventArgs e)
		{
			Helpers?.Log(nameof(YleTextBox), nameof(YleTextBox_FocusLost), $"USER INPUT: user changed value to {e.Value}. TextBox Name='{Name}'. ID='{Id}'");
			TextIsValid();
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.Value));
		}

		private bool TextIsValid()
		{
			bool isValid = ValidationPredicate(Text);

			ValidationState = isValid ? UIValidationState.Valid : UIValidationState.Invalid;

			return isValid;
		}
	}
}