namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System;

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class WrappedMethodAttribute : Attribute
	{
		private readonly int performanceDropDetectionThresholdInMilliseconds;

		public WrappedMethodAttribute(string className, string methodName, int performanceDropDetectionThresholdInMilliseconds = Int32.MaxValue)
		{
			ClassName = className;
			MethodName = methodName;
			this.performanceDropDetectionThresholdInMilliseconds = performanceDropDetectionThresholdInMilliseconds;
		}

		public string ClassName { get; }

		public string MethodName { get; }

		public TimeSpan PerformanceDropDetectionThreshold => TimeSpan.FromMilliseconds(performanceDropDetectionThresholdInMilliseconds);

		public override string ToString()
		{
			return $"{ClassName}.{MethodName}";
		}
	}
}
