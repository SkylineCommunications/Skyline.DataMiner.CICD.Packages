namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using ColumnFilter = Core.DataMinerSystem.Common.ColumnFilter;
	using ComparisonOperator = Core.DataMinerSystem.Common.ComparisonOperator;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.Enums;

	public class SendPlasmaIntegrationNotificationOldDialog : DebugDialog
	{
		private readonly Label plasmaLabel = new Label("Plasma") { Style = TextStyle.Heading };

		private readonly Label programLabel = new Label("Program Information") { Style = TextStyle.Heading };
		private readonly Label transmissionsLabel = new Label("Transmissions") { Style = TextStyle.Heading };

		private readonly Label noTransmissionsLabel = new Label("No Transmissions");

		private readonly Label idLabel = new Label("ID");
		private readonly YleTextBox idTextBox = new YleTextBox { ValidationText = "Required field", ValidationPredicate = text => !String.IsNullOrWhiteSpace(text), Text = Guid.NewGuid().ToString() };
		private readonly Button randomIdButton = new Button("Enter Random Guid") { Width = 150 };

		private readonly Label plasmaIdLabel = new Label("Plasma ID");
		private readonly YleTextBox plasmaIdTextBox = new YleTextBox { ValidationText = "Required field", ValidationPredicate = text => !String.IsNullOrWhiteSpace(text), Text = Guid.NewGuid().ToString() };
		private readonly Button randomPlasmaIdButton = new Button("Enter Random Guid") { Width = 150 };

		private readonly Label categoryLabel = new Label("Category");
		private readonly DropDown categoryDropDown = new DropDown { Options = Enum.GetNames(typeof(Category)), Selected = Enum.GetName(typeof(Category), Category.Uutiset) };

		private readonly Label subcategoryLabel = new Label("Sub Category");
		private readonly DropDown subcategoryDropDown = new DropDown { Options = Enum.GetNames(typeof(SubCategory)), Selected = Enum.GetName(typeof(SubCategory), SubCategory.Uutisbulletiini) };

		private readonly Label projectNumberLabel = new Label("Project Number");
		private readonly TextBox projectNumberTextBox = new TextBox();

		private readonly Label productNumberLabel = new Label("Product Number");
		private readonly TextBox productNumberTextBox = new TextBox();

		private readonly Label yleIdLabel = new Label("YLE ID");
		private readonly TextBox yleIdTextBox = new TextBox();

		private readonly Label finnishPublicationNameLabel = new Label("Finnish Publication Name");
		private readonly TextBox finnishPublicationNameTextBox = new TextBox();

		private readonly Button sendProgramNotificationButton = new Button("Send Program Notification") { Width = 200 };
		private readonly Button deleteProgramInfoButton = new Button("Delete Program Info") { Width = 200 };

		private readonly Button addTransmissionButton = new Button("Add Transmission");

		private readonly List<TransmissionSection> transmissionSections = new List<TransmissionSection>();

		public SendPlasmaIntegrationNotificationOldDialog(Utilities.Helpers helpers) : base(helpers)
		{
			Title = "Send Plasma Integration Notification";

			Initialize();
			GenerateUi();

			AddTransmissionSection();
		}

		private void Initialize()
		{
			randomIdButton.Pressed += (s, e) =>
			{
				idTextBox.Text = Guid.NewGuid().ToString();
				foreach (var section in transmissionSections) section.ProgramId = idTextBox.Text;
			};

			randomPlasmaIdButton.Pressed += (s, e) => plasmaIdTextBox.Text = Guid.NewGuid().ToString();

			idTextBox.Changed += (s, e) =>
			{
				foreach (var section in transmissionSections) section.ProgramId = idTextBox.Text;
			};

			sendProgramNotificationButton.Pressed += (s, e) => SendProgramNotificationButton_Pressed();
			deleteProgramInfoButton.Pressed += (s, e) => DeleteProgramInfo();
			addTransmissionButton.Pressed += (sender, args) => AddTransmissionSection();
		}

		private void AddTransmissionSection()
		{
			var section = new TransmissionSection(helpers, idTextBox.Text);
			section.RemoveButton.Pressed += RemoveTransmissionSectionButton_Pressed;
			section.UiUpdated += (s, e) => GenerateUi();
			section.ShowRequestResult += (s, e) =>
			{
				ShowRequestResult($"Sent publication event notification", e);
				GenerateUi();
			};

			transmissionSections.Add(section);
			GenerateUi();
		}

		private void RemoveTransmissionSectionButton_Pressed(object sender, EventArgs e)
		{
			transmissionSections.RemoveAll(x => x.RemoveButton.Equals(sender));
			GenerateUi();
		}

		private void SendProgramNotificationButton_Pressed()
		{
			Category category = (Category)Enum.Parse(typeof(Category), categoryDropDown.Selected);
			SubCategory subcategory = (SubCategory)Enum.Parse(typeof(SubCategory), subcategoryDropDown.Selected);

			var parsedProgram = new ParsedProgram()
			{
				Id = idTextBox.Text,
				PlasmaId = plasmaIdTextBox.Text,
				FinnishPublicationName = finnishPublicationNameTextBox.Text,
				ProjectNumber = projectNumberTextBox.Text,
				ProductNumber = productNumberTextBox.Text,
				YleId = yleIdTextBox.Text,
				YleMainCategory = (int)category,
				YleSubCategory = subcategory.GetDescription()
			};

			string notification = parsedProgram.ToNotification();

			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, notification);

			ShowRequestResult($"Sent Parsed Program object notification", notification);
			GenerateUi();
		}

		private void DeleteProgramInfo()
		{
			if (!plasmaIdTextBox.IsValid) return;

			OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
			if (orderManagerElement.PlasmaElement == null || orderManagerElement.PlasmaElement.State != ElementState.Active)
			{
				ShowRequestResult($"Unable to remove Program {plasmaIdTextBox.Text}", "Plasma element was not linked with order manager or not active");
				return;
			}

			// Remove entry from Feenix element
			var programTable = orderManagerElement.PlasmaElement.GetTable(MediagenixWhatsOnProtocol.ProgramsTable.TablePid);
			var programKeys = programTable.QueryData(new ColumnFilter[] { new ColumnFilter
				{
					Pid = MediagenixWhatsOnProtocol.ProgramsTable.Pid.PlasmaId,
					ComparisonOperator = ComparisonOperator.Equal,
					Value = plasmaIdTextBox.Text
				}
			}).Select(x => Convert.ToString(x[0])).ToList();

			foreach (var key in programKeys)
			{
				programTable.DeleteRow(key);
				orderManagerElement.Element.SetParameter(130, key);
				ShowRequestResult($"Removed Live Stream Order {key}", "Removed");
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(plasmaLabel, ++row, 0, 1, 2);

			AddWidget(programLabel, ++row, 0, 1, 2);

			AddWidget(idLabel, ++row, 0);
			AddWidget(idTextBox, row, 1);
			AddWidget(randomIdButton, row, 2);

			AddWidget(plasmaIdLabel, ++row, 0);
			AddWidget(plasmaIdTextBox, row, 1);
			AddWidget(randomPlasmaIdButton, row, 2);

			AddWidget(finnishPublicationNameLabel, ++row, 0);
			AddWidget(finnishPublicationNameTextBox, row, 1);

			AddWidget(projectNumberLabel, ++row, 0);
			AddWidget(projectNumberTextBox, row, 1);

			AddWidget(productNumberLabel, ++row, 0);
			AddWidget(productNumberTextBox, row, 1);

			AddWidget(yleIdLabel, ++row, 0);
			AddWidget(yleIdTextBox, row, 1);

			AddWidget(categoryLabel, ++row, 0);
			AddWidget(categoryDropDown, row, 1);

			AddWidget(subcategoryLabel, ++row, 0);
			AddWidget(subcategoryDropDown, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			row = GenerateUi_AddTransmissionSections(row);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendProgramNotificationButton, ++row, 0, 1, 5);

			AddWidget(deleteProgramInfoButton, ++row, 0, 1, 5);

			AddResponseSections(row + 1);
		}

		private int GenerateUi_AddTransmissionSections(int row)
		{
			AddWidget(transmissionsLabel, ++row, 0, 1, 2);
			if (transmissionSections.Any())
			{
				foreach (var section in transmissionSections)
				{
					AddSection(section, new SectionLayout(++row, 0));
					row += section.RowCount;
				}
			}
			else
			{
				AddWidget(noTransmissionsLabel, ++row, 0, 1, 2);
			}

			AddWidget(addTransmissionButton, ++row, 0, 1, 2);

			return row;
		}

		private sealed class TransmissionSection : Section
		{
			private readonly Utilities.Helpers helpers;

			private readonly Label idLabel = new Label("Transmission ID");
			private readonly YleTextBox idTextBox = new YleTextBox { ValidationText = "Required field", ValidationPredicate = text => !String.IsNullOrWhiteSpace(text), Text = Guid.NewGuid().ToString() };
			private readonly Button randomIdButton = new Button("Enter Random Guid") { Width = 150 };

			private readonly Label startLabel = new Label("Start");
			private readonly DateTimePicker startDateTimePicker = new DateTimePicker();

			private readonly Label endLabel = new Label("End");
			private readonly DateTimePicker endDateTimePicker = new DateTimePicker();

			private readonly Label sourceLabel = new Label("Source");
			private readonly TextBox sourceTextBox = new TextBox();

			private readonly Label channelLabel = new Label("Channel");
			private readonly TextBox channelTextBox = new TextBox();

			private readonly Label originalTxLabel = new Label("Original Tx");
			private readonly TextBox originalTxTextBox = new TextBox();

			private readonly Label typeLabel = new Label("Type");
			private readonly DropDown typeDropDown = new DropDown { Options = Enum.GetNames(typeof(TransmissionType)), Selected = TransmissionType.Linear.ToString() };

			private readonly Label isLiveLabel = new Label("Is Live");
			private readonly CheckBox isLiveCheckBox = new CheckBox { IsChecked = true };

			private readonly Button sendTransmissionNotificationButton = new Button("Send Transmission Notification") { Width = 200 };

			public Button RemoveButton { get; private set; } = new Button("Remove Transmission");

			public string ProgramId { get; set; }

			public TransmissionSection(Utilities.Helpers helpers, string programId)
			{
				this.helpers = helpers;
				ProgramId = programId;

				Initialize();
				GenerateUi();
			}

			public event EventHandler<string> ShowRequestResult;

			public event EventHandler UiUpdated;

			//public Transmission ToAsset()
			//{
			//	TransmissionType type = (TransmissionType)Enum.Parse(typeof(TransmissionType), typeDropDown.Selected);

			//	return new Transmission(idTextBox.Text)
			//	{
			//		IsLive = isLiveCheckBox.IsChecked,
			//		Start = startDateTimePicker.DateTime,
			//		End = endDateTimePicker.DateTime,
			//		OriginalTx = originalTxTextBox.Text,
			//		Channel = channelTextBox.Text,
			//		Type = type,
			//		Source = sourceTextBox.Text
			//	};
			//}

			private void Initialize()
			{
				randomIdButton.Pressed += (s, e) => idTextBox.Text = Guid.NewGuid().ToString();
				sendTransmissionNotificationButton.Pressed += SendTransmissionNotificationButton_Pressed;
			}

			private void SendTransmissionNotificationButton_Pressed(object sender, EventArgs e)
			{
				var parsedPublicationEvent = new ParsedPublicationEvent
				{
					Channel = channelTextBox.Text,
					Id = idTextBox.Text,
					ProgramId = ProgramId,
					Live = Convert.ToInt32(isLiveCheckBox.IsChecked),
					Source = sourceTextBox.Text,
					Start = startDateTimePicker.DateTime,
					End = endDateTimePicker.DateTime,
					Type = typeDropDown.Selected,
					Status = "Created"
				};

				string publicationEventNotification = parsedPublicationEvent.ToNotification();

				DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.PlasmaElement, MediagenixWhatsOnProtocol.RabbitMqMessagePid, publicationEventNotification);

				ShowRequestResult?.Invoke(this, publicationEventNotification);
				GenerateUi();
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;

				AddWidget(idLabel, ++row, 0);
				AddWidget(idTextBox, row, 1);
				AddWidget(randomIdButton, row, 2);

				AddWidget(startLabel, ++row, 0);
				AddWidget(startDateTimePicker, row, 1);

				AddWidget(endLabel, ++row, 0);
				AddWidget(endDateTimePicker, row, 1);

				AddWidget(typeLabel, ++row, 0);
				AddWidget(typeDropDown, row, 1);

				AddWidget(isLiveLabel, ++row, 0);
				AddWidget(isLiveCheckBox, row, 1);

				AddWidget(sourceLabel, ++row, 0);
				AddWidget(sourceTextBox, row, 1);

				AddWidget(channelLabel, ++row, 0);
				AddWidget(channelTextBox, row, 1);

				AddWidget(originalTxLabel, ++row, 0);
				AddWidget(originalTxTextBox, row, 1);

				AddWidget(sendTransmissionNotificationButton, ++row, 0, 1, 2);
				AddWidget(RemoveButton, row + 1, 0, 1, 2);

				// Call event to update parent dialog
				UiUpdated?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}