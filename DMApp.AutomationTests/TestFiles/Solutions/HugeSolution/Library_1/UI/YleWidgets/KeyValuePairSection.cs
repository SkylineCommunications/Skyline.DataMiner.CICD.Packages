namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class KeyValuePairSection<TKey, TValue> : Section
	{
		private readonly Button deleteButton = new Button("Delete");

		private IYleInteractiveWidget keyWidget;
		private IYleInteractiveWidget valueWidget;

		public KeyValuePairSection(TKey key = default(TKey), TValue value = default(TValue))
		{
			Initialize(key, value);
			GenerateUi();
		}

		public KeyValuePair<TKey, TValue> KeyValuePair => new KeyValuePair<TKey, TValue>((TKey)keyWidget.Value, (TValue)valueWidget.Value);

		public event EventHandler Deleted;

		private void Initialize(TKey key, TValue value)
		{
			if (!Mapping.TypeToWidget.ContainsKey(typeof(TKey))) throw new ArgumentException("Unknown Key Type");
			if (!Mapping.TypeToWidget.ContainsKey(typeof(TValue))) throw new ArgumentException("Unknown Value Type");

			keyWidget = Mapping.TypeToWidget[typeof(TKey)].Invoke();
			keyWidget.Value = key;

			valueWidget = Mapping.TypeToWidget[typeof(TValue)].Invoke();
			valueWidget.Value = value;

			deleteButton.Pressed += (s, e) => Deleted?.Invoke(this, EventArgs.Empty);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(keyWidget as Widget, ++row, 0);
			AddWidget(valueWidget as Widget, row, 1);
			AddWidget(deleteButton, row, 2);
		}
	}
}
