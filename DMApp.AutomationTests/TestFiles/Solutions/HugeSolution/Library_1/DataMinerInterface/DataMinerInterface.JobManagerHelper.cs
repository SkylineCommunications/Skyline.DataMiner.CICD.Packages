namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System.Collections.Generic;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public static partial class DataMinerInterface
	{
		public static class JobManagerHelper
		{
			[WrappedMethod("JobManagerHelper", "ReadJobs")]
			public static List<Job> ReadJobs(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, FilterElement<Job> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var jobs = jobManagerHelper.Jobs.Read(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return jobs;
			}

			[WrappedMethod("JobManagerHelper", "CreateJob")]
			public static Job CreateJob(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, Job job)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var createdJob = jobManagerHelper.Jobs.Create(job);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return createdJob;
			}

			[WrappedMethod("JobManagerHelper", "UpdateJob")]
			public static Job UpdateJob(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, Job job)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var updatedJob = jobManagerHelper.Jobs.Update(job);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return updatedJob;
			}

			[WrappedMethod("JobManagerHelper", "StitchJob")]
			public static void StitchJob(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, Job job)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				jobManagerHelper.StitchJob(job);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("JobManagerHelper", "StitchJobs")]
			public static void StitchJobs(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, List<Job> jobs)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				jobManagerHelper.StitchJobs(jobs);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("JobManagerHelper", "ReadSectionDefinitions")]
			public static List<SectionDefinition> ReadSectionDefinitions(Helpers helpers, Skyline.DataMiner.Net.Jobs.JobManagerHelper jobManagerHelper, FilterElement<SectionDefinition> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var sectionDefinitions = jobManagerHelper.SectionDefinitions.Read(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return sectionDefinitions;
			}
		}
	}
}
