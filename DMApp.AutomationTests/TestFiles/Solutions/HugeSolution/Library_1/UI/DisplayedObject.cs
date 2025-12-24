namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class DisplayedObject 
	{
		private Dictionary<string, ValidationInfo> propertyValidations = new Dictionary<string, ValidationInfo>();

        public void SetPropertyValidation(string propertyName, UIValidationState validationState, string validationText = null)
        {
            SetPropertyValidation(propertyName, validationState != UIValidationState.Invalid, validationText);
        }

        public void SetPropertyValidation(string propertyName, bool isValid, string validationText = null)
        {
            if (!propertyValidations.TryGetValue(propertyName, out var validation))
            {
                validation = new ValidationInfo();
            }

            validation.SetValidationInfo(isValid ? UIValidationState.Valid : UIValidationState.Invalid, validationText);

            propertyValidations[propertyName] = validation;
        }

        public ValidationInfo GetPropertyValidation(string propertyName)
        {
            if (!propertyValidations.TryGetValue(propertyName, out var validation))
            {
                validation = new ValidationInfo();
                propertyValidations.Add(propertyName, validation);
            }

            return validation;
        }

        public void SetPropertyValue(Helpers helpers, string propertyName, object valueToSet)
		{
            var property = this.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public) ?? throw new PropertyNotFoundException(propertyName, this.GetType().Name);

            property.SetValue(this, valueToSet);

            helpers.Log(this.GetType().Name, nameof(SetPropertyValue), $"Set property {propertyName} to {valueToSet}");
        }
    }
}
