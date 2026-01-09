namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System;

	using Skyline.DataMiner.Automation;

	public class ValidationInfo
	{
		public ValidationInfo()
		{
			State = UIValidationState.NotValidated;
			Text = String.Empty;
		}

		public UIValidationState State { get; set; }

		public string Text { get; set; }

		public bool IsValid
		{
			get
			{
				return State != UIValidationState.Invalid;
			}
		}

		public event EventHandler<ValidationInfoChangedEventArgs> ValidationInfoChanged;

		public void SetValidationInfo(UIValidationState state, string text)
		{
			State = state;
			Text = text;
			ValidationInfoChanged?.Invoke(this, new ValidationInfoChangedEventArgs(State, Text));
		}

		public class ValidationInfoChangedEventArgs : EventArgs
		{
			internal ValidationInfoChangedEventArgs(UIValidationState state, string text)
			{
				State = state;
				Text = text;
			}

			public UIValidationState State { get; private set; }

			public string Text { get; private set; }
		}
	}
}