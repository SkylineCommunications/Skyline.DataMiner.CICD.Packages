namespace ShowPlasmaDetails_2.Sections
{
	using Plasma;
	using ShowPlasmaDetails_2.Plasma.Enums;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;

	public class PlasmaProgramTranslationsAndAudioMixingSection : Section
	{
		private readonly CollapseButton translationAndAudioMixingButton;
		private readonly Label titleLabel = new Label("TRANSLATIONS AND AUDIO MIXING") {Style = TextStyle.Bold};

		private readonly Label subtitlingLabel = new Label("SUBTITLING");
		private readonly Label subtitlingCopyRequiredLabel = new Label("SUBTITLING COPY REQUIRED");
		private readonly Label subtitlingCopyTargetLabel = new Label("SUBTITLING COPY TARGET");
		private readonly Label subtitlingCopyFormatLabel = new Label("SUBTITLING COPY FORMAT");
		private readonly Label translationSubtitlingToLanguage1Label = new Label("TRANSLATION SUBTITLING TO LANGUAGE1");
		private readonly Label translationSubtitlingToLanguage2Label = new Label("TRANSLATION SUBTITLING TO LANGUAGE2");
		private readonly Label translationNarrationToLanguage1Label = new Label("TRANSLATION NARRATION TO LANGUAGE1");
		private readonly Label translationNarrationToLanguage2Label = new Label("TRANSLATION NARRATION TO LANGUAGE2");
		private readonly Label translationLowerThirdsToLanguage1Label = new Label("TRANSLATION LOWER THIRDS TO LANGUAGE1");
		private readonly Label translationLowerThirdsToLanguage2Label = new Label("TRANSLATION LOWER THIRDS TO LANGUAGE2");
		private readonly Label translationDubbingToLanguage1Label = new Label("TRANSLATION DUBBING TO LANGUAGE1");
		private readonly Label translationDubbingToLanguage2Label = new Label("TRANSLATION DUBBING TO LANGUAGE2");
		private readonly Label audioMixing1Label = new Label("AUDIO MIXING 1");
		private readonly Label audioMixing2Label = new Label("AUDIO MIXING 2");

		private readonly Label subtitlingValue;
		private readonly Label subtitlingCopyRequiredValue;
		private readonly Label subtitlingCopyTargetValue;
		private readonly Label subtitlingCopyFormatValue;
		private readonly Label translationSubtitlingToLanguage1Value;
		private readonly Label translationSubtitlingToLanguage2Value;
		private readonly Label translationNarrationToLanguage1Value;
		private readonly Label translationNarrationToLanguage2Value;
		private readonly Label translationLowerThirdsToLanguage1Value;
		private readonly Label translationLowerThirdsToLanguage2Value;
		private readonly Label translationDubbingToLanguage1Value;
		private readonly Label translationDubbingToLanguage2Value;
		private readonly Label audioMixing1Value;
		private readonly Label audioMixing2Value;

		public PlasmaProgramTranslationsAndAudioMixingSection(Engine engine, ParsedPlasmaOrder plasmaOrder, int columnOffset = 0)
		{
			subtitlingValue = new UIDetailValueLabel(plasmaOrder.Program.SubtitlingCoordinator);
			subtitlingCopyRequiredValue = new UIDetailValueLabel(plasmaOrder.ProductionJobs?.SubtitleCopyFormat ?? Constants.NotApplicable);
			subtitlingCopyTargetValue = new UIDetailValueLabel(Constants.NotApplicable);
			subtitlingCopyFormatValue = new UIDetailValueLabel(plasmaOrder.ProductionJobs?.SubtitleCopyFormat ?? Constants.NotApplicable);
			translationSubtitlingToLanguage1Value = new UIDetailValueLabel(((SubtitlesAvailable)plasmaOrder.Program.TranslationSubtitlingToLanguage1).GetDescription());
			translationSubtitlingToLanguage2Value = new UIDetailValueLabel(((SubtitlesAvailable)plasmaOrder.Program.TranslationSubtitlingToLanguage2).GetDescription());
			translationNarrationToLanguage1Value = new UIDetailValueLabel(((NarrationAvailable)plasmaOrder.Program.TranslationNarrationToLanguage1).GetDescription());
			translationNarrationToLanguage2Value = new UIDetailValueLabel(((NarrationAvailable)plasmaOrder.Program.TranslationNarrationToLanguage2).GetDescription());
			translationLowerThirdsToLanguage1Value = new UIDetailValueLabel(((TranslationLowerThirds)plasmaOrder.Program.TranslationLowerThirdsToLanguage1).GetDescription());
			translationLowerThirdsToLanguage2Value = new UIDetailValueLabel(((TranslationLowerThirds)plasmaOrder.Program.TranslationLowerThirdsToLanguage2).GetDescription());
			translationDubbingToLanguage1Value = new UIDetailValueLabel(((TranslationDubbing)plasmaOrder.Program.TranslationDubbingToLanguage1).GetDescription());
			translationDubbingToLanguage2Value = new UIDetailValueLabel(((TranslationDubbing)plasmaOrder.Program.TranslationDubbingToLanguage2).GetDescription());
			audioMixing1Value = new UIDetailValueLabel(((AudioMixing)plasmaOrder.Program.AudioMixing1).GetDescription());
			audioMixing2Value = new UIDetailValueLabel(((AudioMixing)plasmaOrder.Program.AudioMixing2).GetDescription());

			translationAndAudioMixingButton = new CollapseButton(new[]
			{
				subtitlingLabel, subtitlingCopyRequiredLabel, subtitlingCopyTargetLabel, subtitlingCopyFormatLabel, translationSubtitlingToLanguage1Label, translationSubtitlingToLanguage2Label, translationNarrationToLanguage1Label,
				translationNarrationToLanguage2Label, translationLowerThirdsToLanguage1Label, translationLowerThirdsToLanguage2Label, translationDubbingToLanguage1Label, translationDubbingToLanguage2Label, audioMixing1Label, audioMixing2Label,
				subtitlingValue, subtitlingCopyRequiredValue, subtitlingCopyTargetValue, subtitlingCopyFormatValue, translationSubtitlingToLanguage1Value, translationSubtitlingToLanguage2Value, translationNarrationToLanguage1Value,
				translationNarrationToLanguage2Value, translationLowerThirdsToLanguage1Value, translationLowerThirdsToLanguage2Value, translationDubbingToLanguage1Value, translationDubbingToLanguage2Value, audioMixing1Value, audioMixing2Value
			}, true) {CollapseText = "-", ExpandText = "+", Width = 44};

			GenerateUI(columnOffset);
		}

		private void GenerateUI(int columnOffset)
		{
			var row = 0;

			AddWidget(translationAndAudioMixingButton, new WidgetLayout(row, 0));
			AddWidget(titleLabel, new WidgetLayout(row, 1, 1, 5 + columnOffset));

			AddWidget(subtitlingLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(subtitlingValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(subtitlingCopyRequiredLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(subtitlingCopyRequiredValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(subtitlingCopyTargetLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(subtitlingCopyTargetValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(subtitlingCopyFormatLabel, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(subtitlingCopyFormatValue, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationSubtitlingToLanguage1Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationSubtitlingToLanguage1Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationSubtitlingToLanguage2Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationSubtitlingToLanguage2Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationNarrationToLanguage1Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationNarrationToLanguage1Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationNarrationToLanguage2Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationNarrationToLanguage2Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationLowerThirdsToLanguage1Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationLowerThirdsToLanguage1Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationLowerThirdsToLanguage2Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationLowerThirdsToLanguage2Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationDubbingToLanguage1Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationDubbingToLanguage1Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(translationDubbingToLanguage2Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(translationDubbingToLanguage2Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(audioMixing1Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(audioMixing1Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));

			AddWidget(audioMixing2Label, new WidgetLayout(++row, 1, 1, 3 + columnOffset));
			AddWidget(audioMixing2Value, new WidgetLayout(row, 5 + columnOffset, 1, 1));
		}

		public Label Title => titleLabel;

		public CollapseButton CollapseButton => translationAndAudioMixingButton;
	}
}