namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using System.Diagnostics;
	using NPOI.OpenXmlFormats.Dml;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	public class JobManager
	{
		private readonly Helpers helpers;
		private readonly JobManagerHelper jobHelper;

		private IEnumerable<SectionDefinition> allSectionDefinitions;

		public JobManager(Connection connection, Helpers helpers)
		{
			this.helpers = helpers;
			jobHelper = new JobManagerHelper(m => connection.HandleMessages(m));
		}

		public IEnumerable<SectionDefinition> AllSectionDefinitions => allSectionDefinitions ?? (allSectionDefinitions = DataMinerInterface.JobManagerHelper.ReadSectionDefinitions(helpers, jobHelper, SectionDefinitionExposers.Name.NotEqual(string.Empty)));

		public Job GetJob(Guid id)
		{
			return GetJob(JobExposers.ID.Equal(id));
		}

		public Job GetJob(FilterElement<Job> filter)
		{
			LogMethodStart(nameof(GetJob), out var stopwatch);

			var job = DataMinerInterface.JobManagerHelper.ReadJobs(helpers, jobHelper, filter).FirstOrDefault();
			if (job == null)
			{
				LogMethodCompleted(nameof(GetJob), stopwatch);
				return null;
			}

			DataMinerInterface.JobManagerHelper.StitchJob(helpers, jobHelper, job);

			LogMethodCompleted(nameof(GetJob), stopwatch);

			return job;
		}

		public List<Job> GetJobs(FilterElement<Job> filter)
		{
			LogMethodStart(nameof(GetJobs), out var stopwatch);

			var jobs = DataMinerInterface.JobManagerHelper.ReadJobs(helpers, jobHelper, filter);
			if (jobs == null) return new List<Job>();

			DataMinerInterface.JobManagerHelper.StitchJobs(helpers, jobHelper, jobs);

			LogMethodCompleted(nameof(GetJobs), stopwatch);

			return jobs;
		}

		public bool TryAddJob(Job job, out Job resultingJob)
		{
			resultingJob = null;

			try
			{
				resultingJob = DataMinerInterface.JobManagerHelper.CreateJob(helpers, jobHelper, job);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryAddJob), "Something went wrong: " + e);
				return false;
			}
		}

		public bool TryUpdateJob(Job job, out Job resultingJob)
		{
			resultingJob = null;

			try
			{
				resultingJob = DataMinerInterface.JobManagerHelper.UpdateJob(helpers, jobHelper, job);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryUpdateJob), $"Something went wrong: {e}");
				return false;
			}
		}

		public bool DeleteJob(Job job)
		{
			try
			{
				jobHelper.Jobs.Delete(job);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool HasReservation(Job job, SectionDefinition sectionDefinition, FieldDescriptor reservationFieldDescriptor, Guid reservationId)
		{
			return GetReservationIdsLinkedToJob(job, sectionDefinition, reservationFieldDescriptor).Contains(reservationId);
		}

		public bool HasReservations(Job job, SectionDefinition sectionDefinition, FieldDescriptor reservationFieldDescriptor)
		{
			return GetReservationIdsLinkedToJob(job, sectionDefinition, reservationFieldDescriptor).Any();
		}

		public List<Guid> GetReservationIdsLinkedToJob(Job job, SectionDefinition sectionDefinition, FieldDescriptor reservationFieldDescriptor)
		{
			var reservationIds = new List<Guid>();

			foreach (var section in job.Sections)
			{
				if (!section.SectionDefinitionID.Equals(sectionDefinition.GetID())) continue;

				var fieldValue = section.GetFieldValueById(reservationFieldDescriptor.ID);
				if (fieldValue?.Value == null) continue;

				if (fieldValue.Value.Type != typeof(Guid)) continue;
				Guid reservationId = (Guid)fieldValue.Value.Value;

				if (reservationId == Guid.Empty) continue;

				reservationIds.Add(reservationId);
			}

			return reservationIds;
		}

		public JobDomain GetDefaultJobDomain()
		{
			var eventSectionDefinition = GetCustomEventSectionDefinition();
			if (eventSectionDefinition == null) return null;

			var eventSectionDefinitionId = eventSectionDefinition.GetID();
			if (eventSectionDefinitionId == null) return null;

			var jobDomains = jobHelper.JobDomains.Read(JobDomainExposers.SectionDefinitionIDs.Contains(eventSectionDefinitionId.Id));
			if (jobDomains == null) return null;

			return jobDomains.FirstOrDefault();
		}

		public bool UpdateSectionDefinition(SectionDefinition sectionDefinition)
		{
			LogMethodStart(nameof(UpdateSectionDefinition), out var stopwatch);

			try
			{
				jobHelper.SectionDefinitions.Update(sectionDefinition);

				LogMethodCompleted(nameof(UpdateSectionDefinition), stopwatch);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void UpdateSectionField(Section section, SectionDefinition sectionDefinition, string name, string value)
		{
			if (section == null) throw new ArgumentNullException(nameof(section));
			if (sectionDefinition == null) throw new ArgumentNullException(nameof(sectionDefinition));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

			var fieldDescriptor = GetFieldDescriptor(sectionDefinition, name);
			if (fieldDescriptor == null) return;

			section.AddOrReplaceFieldValue(new FieldValue(fieldDescriptor) { Value = new ValueWrapper<string>(value) });
		}

		public SectionDefinition GetStaticEventSectionDefinition()
		{
			LogMethodStart(nameof(GetStaticEventSectionDefinition), out var stopwatch);

			var staticEventSectionDefinition = AllSectionDefinitions.FirstOrDefault(x => x.GetAllFieldDescriptors().Any(y => y.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)));

			LogMethodCompleted(nameof(GetStaticEventSectionDefinition), stopwatch);

			return staticEventSectionDefinition;
		}

		public SectionDefinition GetCustomEventSectionDefinition()
		{
			LogMethodStart(nameof(GetCustomEventSectionDefinition), out var stopwatch);

			var customEventSectionDefinition = AllSectionDefinitions.FirstOrDefault(x => x.GetAllFieldDescriptors().Any(y => y.Name.Equals("Status", StringComparison.OrdinalIgnoreCase)));

			LogMethodCompleted(nameof(GetCustomEventSectionDefinition), stopwatch);

			// search for a section with a field named Status
			return customEventSectionDefinition;
		}

		public SectionDefinition GetOrderSectionDefinition()
		{
			LogMethodStart(nameof(GetOrderSectionDefinition), out var stopwatch);

			var orderSectionDefinition = AllSectionDefinitions.FirstOrDefault(sd => sd.GetName().Equals("Orders", StringComparison.OrdinalIgnoreCase));

			LogMethodCompleted(nameof(GetOrderSectionDefinition), stopwatch);

			return orderSectionDefinition;
		}

		public Section GetStaticEventSection(Job job)
		{
			var allSections = job.Sections;
			if (allSections == null) return null;

			return allSections.FirstOrDefault(x => x.GetSectionDefinition().GetAllFieldDescriptors().Any(y => y.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)));
		}

		public Section GetCustomEventSection(Job job)
		{
			var allSections = job.Sections;
			if (allSections == null) return null;

			return allSections.FirstOrDefault(x => x.GetSectionDefinition().GetAllFieldDescriptors().Any(y => y.Name.Equals("Status", StringComparison.OrdinalIgnoreCase)));
		}

		public Section GetCustomOrdersSection(Job job)
		{
			var sections = job.Sections.Where(x => x.GetSectionDefinition().GetName().Equals("Orders", StringComparison.OrdinalIgnoreCase));
			return sections.FirstOrDefault();
		}

		public Section GetSectionForReservationId(Job job, SectionDefinition sectionDefinition, FieldDescriptor reservationFieldDescriptor, Guid orderId)
		{
			foreach (var section in job.Sections)
			{
				if (!section.SectionDefinitionID.Equals(sectionDefinition.GetID())) continue;

				var fieldValue = section.GetFieldValueById(reservationFieldDescriptor.ID);
				if (fieldValue?.Value == null) continue;

				if (fieldValue.Value.Type != typeof(Guid)) continue;

				if ((Guid)fieldValue.Value.Value == orderId) return section;
			}

			return null;
		}

		public FieldDescriptor GetFieldDescriptor(SectionDefinition sectionDefinition, string name)
		{
			if (sectionDefinition == null) throw new ArgumentNullException(nameof(sectionDefinition));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

			var sectionDefinitionFieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
			return sectionDefinitionFieldDescriptors.FirstOrDefault(d => String.Equals(d.Name, name));
		}

		public ReservationFieldDescriptor GetOrCreateOrderReservationIdFieldDescriptor(SectionDefinition sectionDefinition)
		{
			var fieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
			var reservationIdFieldDescriptor = fieldDescriptors.FirstOrDefault(f => f is ReservationFieldDescriptor);
			if (reservationIdFieldDescriptor == null)
			{
				var customSectionDefinition = sectionDefinition as CustomSectionDefinition ?? throw new NotFoundException("Unable to find field descriptor");

				reservationIdFieldDescriptor = new ReservationFieldDescriptor
				{
					FieldType = typeof(Guid),
					Name = Event.OrderReservationFieldDescriptorName,
					IsHidden = false,
					IsOptional = true
				};

				customSectionDefinition.AddOrReplaceFieldDescriptor(reservationIdFieldDescriptor);
				UpdateSectionDefinition(customSectionDefinition);
			}

			return reservationIdFieldDescriptor as ReservationFieldDescriptor;

			/*
			VSC 04/11/2021: No longer necessary 

            var newReservationFieldDescriptor = new ReservationFieldDescriptor
            {
                FieldType = typeof(Guid),
                Name = Event.OrderReservationFieldDescriptorName,
                IsHidden = false,
                IsOptional = true
            };

            sectionDefinition.AddOrReplaceFieldDescriptor(newReservationFieldDescriptor);
            UpdateSectionDefinition(sectionDefinition);
			*/
		}

		public FieldDescriptor GetOrCreateOrderIsIntegrationFieldDescriptor(SectionDefinition sectionDefinition)
		{
			var fieldDescriptors = sectionDefinition.GetAllFieldDescriptors();
			var orderIsIntegrationFieldDescriptor = fieldDescriptors.FirstOrDefault(f => f.Name == Event.OrderIsIntegrationFieldDescriptorName);
			if (orderIsIntegrationFieldDescriptor == null)
			{
				var customSectionDefinition = sectionDefinition as CustomSectionDefinition ?? throw new NotFoundException("Unable to find section definition");

				orderIsIntegrationFieldDescriptor = new FieldDescriptor
				{
					FieldType = typeof(bool),
					Name = Event.OrderIsIntegrationFieldDescriptorName,
					IsHidden = false,
					IsOptional = true
				};

				customSectionDefinition.AddOrReplaceFieldDescriptor(orderIsIntegrationFieldDescriptor);
				UpdateSectionDefinition(customSectionDefinition);
			}

			return orderIsIntegrationFieldDescriptor;

			/*
			VSC 04/11/2021: No longer necessary 


	        var newOrderIsIntegrationFieldDescriptor = new FieldDescriptor
	        {
		        FieldType = typeof(bool),
		        Name = Event.OrderIsIntegrationFieldDescriptorName,
		        IsHidden = false,
		        IsOptional = true
	        };

	        sectionDefinition.AddOrReplaceFieldDescriptor(newOrderIsIntegrationFieldDescriptor);
	        UpdateSectionDefinition(sectionDefinition);
			*/
		}

		public List<string> GetAttachments(JobID jobId)
		{
			return jobHelper.Jobs.GetJobAttachmentFileNames(jobId);
		}

		public byte[] GetAttachment(JobID jobId, string fileName)
		{
			return jobHelper.Jobs.GetJobAttachment(jobId, fileName);
		}

		public void AddAttachment(JobID jobId, string fileName, byte[] fileContent)
		{
			jobHelper.Jobs.AddJobAttachment(jobId, fileName, fileContent);
		}

		public void DeleteAttachments(JobID jobId, string fileName)
		{
			jobHelper.Jobs.DeleteJobAttachment(jobId, fileName);
		}

		internal void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			helpers.LogMethodStart(nameof(JobManager), nameOfMethod, out stopwatch);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(nameof(JobManager), nameOfMethod, null, stopwatch);
		}

		protected void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(nameof(EventManager), nameOfMethod, message, nameOfObject);
		}
	}
}