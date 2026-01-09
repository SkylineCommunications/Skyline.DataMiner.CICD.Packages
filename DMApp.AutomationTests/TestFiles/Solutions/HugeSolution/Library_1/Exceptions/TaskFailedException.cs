namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	[Serializable]
	public class TaskFailedException : Exception
	{
		public TaskFailedException()
		{
		}

		public TaskFailedException(Task task)
			: base($"Task {task.Description} failed")
		{
		}

		public TaskFailedException(IEnumerable<Task> tasks)
			: base($"The following tasks failed: {String.Join(", ", tasks.Select(x => x.Description))}")
		{
		}

		public TaskFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected TaskFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
