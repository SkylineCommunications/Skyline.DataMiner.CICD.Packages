namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.BookingManagers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public class RemoveDuplicatePropertiesDialog : DebugDialog
	{
		private readonly List<BookingManagerPropertiesSection> bookingManagerSections = new List<BookingManagerPropertiesSection>();

		public RemoveDuplicatePropertiesDialog(Helpers helpers) : base(helpers)
		{
			Title = "Remove Duplicate Booking Manager Properties";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			var bookingManagers = Automation.Engine.SLNetRaw.GetDms().GetElements().Where(x => x.Protocol.Name.Equals("Skyline Booking Manager") && x.State == ElementState.Active).ToList();
			foreach (var bookingManager in bookingManagers)
			{
				bookingManagerSections.Add(new BookingManagerPropertiesSection(bookingManager));
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = 0;
			foreach (var section in bookingManagerSections)
			{
				AddSection(section, new SectionLayout(++row, 0));
				row += section.RowCount;
			}

			SetColumnWidth(0, 40);
		}

		private class BookingManagerPropertiesSection : Section
		{
			private readonly IDmsElement element;
			private readonly List<PropertySection> duplicatePropertySections = new List<PropertySection>();

			public BookingManagerPropertiesSection(IDmsElement element)
			{
				this.element = element ?? throw new ArgumentNullException(nameof(element));

				Initialize();
				GenerateUi();
			}

			private void Initialize()
			{
				DeleteAllDuplicatesButton.Pressed += DeleteAllDuplicatesButton_Pressed;
				FindDuplicateProperties();
			}

			private void FindDuplicateProperties()
			{
				duplicatePropertySections.Clear();

				IDmsTable propertiesTable = element.GetTable(4000);

				var properties = propertiesTable.GetData().Select(x => new Property { Key = Convert.ToString(x.Value[0]), Name = Convert.ToString(x.Value[1]) }).ToList();
				HashSet<Property> duplicateProperties = new HashSet<Property>();
				foreach (var property in properties)
				{
					if (properties.Count(x => String.Equals(x.Name, property.Name)) > 1)
					{
						duplicateProperties.Add(property);
					}
				}

				foreach (var duplicateProperty in duplicateProperties)
				{
					var section = new PropertySection(duplicateProperty);
					section.DeleteButtonPressed += Section_DeleteButtonPressed;
					duplicatePropertySections.Add(section);
				}
			}

			private void DeleteAllDuplicatesButton_Pressed(object sender, EventArgs e)
			{
				foreach (var section in duplicatePropertySections)
				{
					element.GetTable(4000).DeleteRow(section.Property.Key);
				}

				FindDuplicateProperties();
				GenerateUi();
			}

			private void Section_DeleteButtonPressed(object sender, string rowKey)
			{
				element.GetTable(4000).DeleteRow(rowKey);

				FindDuplicateProperties();
				GenerateUi();
			}

			private void GenerateUi()
			{
				Clear();

				int row = -1;
				AddWidget(new Label(element.Name) { Style = TextStyle.Bold }, ++row, 0, 1, 4);
				if (!duplicatePropertySections.Any())
				{
					AddWidget(new Label("This booking manager does not contain duplicate properties") { Style = TextStyle.Bold }, ++row, 1, 1, 3);
				}
				else
				{
					foreach (var duplicatePropertySection in duplicatePropertySections)
					{
						AddSection(duplicatePropertySection, new SectionLayout(++row, 1));
						row += duplicatePropertySection.RowCount;
					}

					AddWidget(DeleteAllDuplicatesButton, ++row, 1, 1, 3);
				}
			}

			public Button DeleteAllDuplicatesButton { get; } = new Button("Delete All Duplicates") { Style = ButtonStyle.CallToAction };
		}

		private class Property
		{
			public string Name { get; set; }

			public string Key { get; set; }

			public override bool Equals(object obj)
			{
				Property other = obj as Property;
				if (other == null) return false;
				return String.Equals(Name, other.Name);
			}

			public override int GetHashCode()
			{
				return Name.GetHashCode();
			}
		}

		private class PropertySection : Section
		{
			private readonly Property property;
			private readonly Button deleteButton = new Button("Delete");

			public PropertySection(Property property)
			{
				this.property = property ?? throw new ArgumentNullException(nameof(property));

				Initialize();
				GenerateUi();
			}

			private void Initialize()
			{
				deleteButton.Pressed += (s, e) => DeleteButtonPressed?.Invoke(this, property.Key);
			}

			private void GenerateUi()
			{
				AddWidget(new Label(property.Key), 0, 0);
				AddWidget(new Label(property.Name), 0, 1);
				AddWidget(deleteButton, 0, 2);
			}

			public Property Property => property;

			public event EventHandler<string> DeleteButtonPressed;
		}
	}
}
