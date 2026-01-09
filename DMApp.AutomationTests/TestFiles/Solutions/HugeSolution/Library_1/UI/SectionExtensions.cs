namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class SectionExtensions
	{
        public static string GetDisplayedPropertyName(this Section section, string fieldName)
		{
            var field = section.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            var displaysPropertyAttribute = field.GetCustomAttributes(typeof(DisplaysPropertyAttribute), true).OfType<DisplaysPropertyAttribute>().SingleOrDefault();

            return displaysPropertyAttribute?.PropertyName;
        }

        public static void SubscribeToDisplayedObjectValidation(this Section section, DisplayedObject displayedObject, Helpers helpers = null)
		{
            var fields = section.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            helpers?.Log(section.GetType().Name, nameof(SubscribeToDisplayedObjectValidation), $"Found fields and properties '{string.Join(", ", fields.Select(x => x.Name))}'");

            foreach (var field in fields)
            {
                var displaysPropertyAttribute = field.GetCustomAttributes(typeof(DisplaysPropertyAttribute), true).OfType<DisplaysPropertyAttribute>().SingleOrDefault();
                if (displaysPropertyAttribute is null) continue;

                var validation = displayedObject.GetPropertyValidation(displaysPropertyAttribute.PropertyName);

                var value = field.GetValue(section);

                if (value is TextBox textBox)
                {
                    validation.ValidationInfoChanged += (s, v) =>
                    {
                        textBox.ValidationState = v.State;
                        textBox.ValidationText = v.Text;
                    };
                }
                else if (value is DateTimePicker dateTimePicker)
                {
                    validation.ValidationInfoChanged += (s, v) =>
                    {
                        dateTimePicker.ValidationState = v.State;
                        dateTimePicker.ValidationText = v.Text;
                    };
                }
                else if (value is DropDown dropDown)
                {
                    validation.ValidationInfoChanged += (s, v) =>
                    {
                        dropDown.ValidationState = v.State;
                        dropDown.ValidationText = v.Text;
                    };
                }
                else
				{
                    helpers?.Log(section.GetType().Name, nameof(SubscribeToDisplayedObjectValidation), $"No validation available for field or property {field.Name}");
                }
            }
        }
	}
}
