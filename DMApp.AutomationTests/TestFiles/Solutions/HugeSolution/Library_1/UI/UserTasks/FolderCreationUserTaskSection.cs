namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class FolderCreationUserTaskSection : NonLiveUserTaskSection
    {
        private readonly Label producerEmailLabel = new Label("Producer Email");
        private readonly Label mediaManagerEmailLabel = new Label("Media Manager Email");
        private readonly Label programNameLabel = new Label("Program Name");
        private readonly Label episodeNumberLabel = new Label("Episode Number");
        private readonly Label productOrProductionNumberLabel = new Label("Product or Production Number");
        private readonly Label destinationLabel = new Label("Destination");

        private readonly IplayFolderCreationUserTask folderCreationUserTask;

        public FolderCreationUserTaskSection(Helpers helpers, IplayFolderCreationUserTask folderCreationUserTask) : base(helpers)
        {
            this.folderCreationUserTask = folderCreationUserTask ?? throw new ArgumentNullException(nameof(folderCreationUserTask));

            ProducerEmailTextBox.Text = folderCreationUserTask.ProducerEmail;
            MediaManagerEmailTextBox.Text = folderCreationUserTask.MediaManagerEmail;
            ProgramNameTextBox.Text = folderCreationUserTask.ProgramName;
            EpisodeNumberTextBox.Text = folderCreationUserTask.EpisodeNumberOrName;
            ProductOrProductionNumberTextBox.Text = folderCreationUserTask.ProductOrProductionNumber;
            DestinationDropDown.Selected = folderCreationUserTask.Destination;

            GenerateUI();
            UpdateVisibility();
        }

        public YleTextBox ProducerEmailTextBox { get; private set; } = new YleTextBox();

        public YleTextBox MediaManagerEmailTextBox { get; private set; } = new YleTextBox();

        public YleTextBox ProgramNameTextBox { get; private set; } = new YleTextBox();

        public YleTextBox EpisodeNumberTextBox { get; private set; } = new YleTextBox();

        public YleTextBox ProductOrProductionNumberTextBox { get; private set; } = new YleTextBox();

        public YleDropDown DestinationDropDown { get; private set; } = new YleDropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>());

        public override void GenerateUI()
        {
            int row = -1;

            AddWidget(programNameLabel, ++row, 0);
            AddWidget(ProgramNameTextBox, row, 1);

            AddWidget(episodeNumberLabel, ++row, 0);
            AddWidget(EpisodeNumberTextBox, row, 1);

            AddWidget(producerEmailLabel, ++row, 0);
            AddWidget(ProducerEmailTextBox, row, 1);

            AddWidget(mediaManagerEmailLabel, ++row, 0);
            AddWidget(MediaManagerEmailTextBox, row, 1);

            AddWidget(productOrProductionNumberLabel, ++row, 0);
            AddWidget(ProductOrProductionNumberTextBox, row, 1);

            AddWidget(destinationLabel, ++row, 0);
            AddWidget(DestinationDropDown, row, 1);
        }

        public override void UpdateUserTask()
        {
            folderCreationUserTask.ProgramName = ProgramNameTextBox.Text;
            folderCreationUserTask.EpisodeNumberOrName = EpisodeNumberTextBox.Text;
            folderCreationUserTask.ProducerEmail = ProducerEmailTextBox.Text;
            folderCreationUserTask.MediaManagerEmail = MediaManagerEmailTextBox.Text;
            folderCreationUserTask.ProductOrProductionNumber = ProductOrProductionNumberTextBox.Text;
            folderCreationUserTask.Destination = DestinationDropDown.Selected;

            folderCreationUserTask.AddOrUpdate(helpers);
        }

        private void UpdateVisibility()
        {
            programNameLabel.IsVisible = !string.IsNullOrWhiteSpace(ProgramNameTextBox.Text);
            ProgramNameTextBox.IsVisible = programNameLabel.IsVisible;

            episodeNumberLabel.IsVisible = !string.IsNullOrWhiteSpace(EpisodeNumberTextBox.Text);
            EpisodeNumberTextBox.IsVisible = episodeNumberLabel.IsVisible;

            producerEmailLabel.IsVisible = !string.IsNullOrWhiteSpace(ProducerEmailTextBox.Text);
            ProducerEmailTextBox.IsVisible = producerEmailLabel.IsVisible;

            mediaManagerEmailLabel.IsVisible = !string.IsNullOrWhiteSpace(MediaManagerEmailTextBox.Text);
            MediaManagerEmailTextBox.IsVisible = mediaManagerEmailLabel.IsVisible;

            productOrProductionNumberLabel.IsVisible = !string.IsNullOrWhiteSpace(ProductOrProductionNumberTextBox.Text);
            ProductOrProductionNumberTextBox.IsVisible = productOrProductionNumberLabel.IsVisible;

            destinationLabel.IsVisible = !string.IsNullOrWhiteSpace(DestinationDropDown.Selected);
            DestinationDropDown.IsVisible = destinationLabel.IsVisible;
        }
    }
}
