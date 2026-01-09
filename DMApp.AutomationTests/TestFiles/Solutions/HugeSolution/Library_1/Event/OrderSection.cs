namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	public class OrderSection : Section
	{
		private ReservationFieldDescriptor reservationFieldDescriptor;
		private FieldDescriptor isIntegrationFieldDescriptor;

		private readonly JobManager jobManager;
		private readonly SectionDefinition orderSectionDefinition;

		private Guid orderId;
		private bool orderIdIsValid;

		private bool orderIsIntegration;
		private bool orderIsIntegrationIsValid;

		public OrderSection(Helpers helpers, LiteOrder order, SectionDefinition orderSectionDefinition)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			this.orderSectionDefinition = orderSectionDefinition ?? throw new ArgumentNullException(nameof(orderSectionDefinition));
			this.jobManager = new JobManager(Engine.SLNetRaw, helpers);

			GetFieldDescriptors();

			Section = new Section(orderSectionDefinition);

			OrderId = order.Id;
			OrderIsIntegration = order.IntegrationType != IntegrationType.None;
		}

		public OrderSection(Section section)
		{
			this.Section = section;

			foreach (var fieldValue in section.FieldValues)
			{
				var fieldDescriptor = fieldValue.GetFieldDescriptor();
				if (fieldDescriptor == null) continue;

				if (fieldDescriptor is ReservationFieldDescriptor)
				{
					if (fieldValue.Value == null || fieldValue.Value.Type != typeof(Guid)) throw new UnexpectedTypeException($"fieldDescriptor {fieldDescriptor.ID} does not contain a Guid");

					orderId = (Guid)fieldValue.Value.Value;
					orderIdIsValid = true;
				}
				else if (fieldDescriptor.Name == Event.OrderIsIntegrationFieldDescriptorName)
				{
					if (fieldValue.Value == null || fieldValue.Value.Type != typeof(bool)) throw new UnexpectedTypeException($"fieldDescriptor {fieldDescriptor.ID} does not contain a boolean");

					orderIsIntegration = (bool)fieldValue.Value.Value;
					orderIsIntegrationIsValid = true;
				}
				else
				{
					// Nothing to do
				}
			}
		}

		public bool IsValid => orderIdIsValid && orderIsIntegrationIsValid;

		public Section Section { get; }

		public Guid OrderId
		{
			get => orderId;

			set
			{
				orderId = value;
				Section.AddOrReplaceFieldValue(new FieldValue(reservationFieldDescriptor) { Value = new ValueWrapper<Guid>(orderId) });
				orderIdIsValid = true;
			}
		}

		public bool OrderIsIntegration
		{
			get => orderIsIntegration;

			set
			{
				orderIsIntegration = value;
				Section.AddOrReplaceFieldValue(new FieldValue(isIntegrationFieldDescriptor) { Value = new ValueWrapper<bool>(orderIsIntegration) });
				orderIsIntegrationIsValid = true;
			}
		}

		private void GetFieldDescriptors()
		{
			reservationFieldDescriptor = jobManager.GetOrCreateOrderReservationIdFieldDescriptor(orderSectionDefinition);
			if (reservationFieldDescriptor == null) throw new FieldDescriptorNotFoundException($"Unable to get or create reservation Id field descriptor");

			isIntegrationFieldDescriptor = jobManager.GetOrCreateOrderIsIntegrationFieldDescriptor(orderSectionDefinition);
			if (isIntegrationFieldDescriptor == null) throw new FieldDescriptorNotFoundException($"Unable to get or create order is integration field descriptor");
		}
	}
}
