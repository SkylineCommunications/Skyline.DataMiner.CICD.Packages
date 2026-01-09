namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class Validator
	{
		protected readonly Helpers helpers;

		protected Validator(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		public List<string> ValidationMessages { get; } = new List<string>();

		public bool Validate()
		{
			helpers.LogMethodStart(this.GetType().Name, nameof(Validate), out var stopwatch);
			ValidationMessages.Clear();

			bool isValid = true;

			var validationSteps = GetValidationSteps();
			foreach (var validationStep in validationSteps)
			{
				isValid &= validationStep.Invoke();
			}

			helpers.LogMethodCompleted(this.GetType().Name, nameof(Validate), null, stopwatch);

			return isValid;
		}

		private List<Func<bool>> GetValidationSteps()
		{
			var validationSteps = new List<Func<bool>>();

			var methods = this.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			foreach (var method in methods)
			{
				bool isValidationStep = method.GetCustomAttributes(typeof(ValidationStepAttribute), false).Any();

				if (isValidationStep)
				{
					var validationStep = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), this, method);

					validationSteps.Add(validationStep);
				}
			}

			return validationSteps;
		}

		protected void Log(string nameOfMethod, string message)
		{
			helpers.Log(this.GetType().Name, nameOfMethod, message);
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ValidationStepAttribute : Attribute
	{
		public ValidationStepAttribute()
		{

		}
	}
}
