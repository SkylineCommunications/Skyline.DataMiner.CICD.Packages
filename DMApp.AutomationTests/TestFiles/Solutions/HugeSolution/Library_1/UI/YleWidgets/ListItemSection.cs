namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ListItemSection<T> : Section
	{
		private readonly Button deleteButton = new Button("Delete");

		private IYleInteractiveWidget valueWidget;

		public ListItemSection(T value = default(T))
		{
			Initialize(value);
			GenerateUi();
		}

		public T Value => (T)valueWidget.Value;

		public event EventHandler Deleted;

		private void Initialize(T value)
		{
			if (!Mapping.TypeToWidget.ContainsKey(typeof(T))) throw new ArgumentException("Unknown Type");

			valueWidget = Mapping.TypeToWidget[typeof(T)].Invoke();
			valueWidget.Value = value;

			deleteButton.Pressed += (s, e) => Deleted?.Invoke(this, EventArgs.Empty);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(valueWidget as Widget, ++row, 0);
			AddWidget(deleteButton, row, 1);
		}
	}
}
