namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static partial class DataMinerInterface
	{
		public static List<WrappedMethodAttribute> GetWrappedMethods()
		{
			var wrappedMethods = new List<WrappedMethodAttribute>();

			var subClasses = typeof(DataMinerInterface).GetNestedTypes();

			foreach (var subClass in subClasses)
			{
				var methods = subClass.GetMethods();

				foreach (var method in methods)
				{
					var wrappedMethodAttribute = (WrappedMethodAttribute) method.GetCustomAttributes(typeof(WrappedMethodAttribute), true).FirstOrDefault();

					if (wrappedMethodAttribute != null)
					{
						wrappedMethods.Add(wrappedMethodAttribute);
					}
				}
			}

			return wrappedMethods;
		}

		private static MetricLogger StartPerformanceLogging(Helpers helpers)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			return MetricLogger.StartNew(helpers, nameof(DataMinerInterface), nameOfMethod);
		}

		private static void LogMethodStart(Helpers helpers, MethodBase methodBase, out Stopwatch stopwatch, string nameOfObject = null)
		{
			var methodName = ((WrappedMethodAttribute)methodBase.GetCustomAttributes(typeof(WrappedMethodAttribute), true)[0]).ToString();

			helpers.LogMethodStart(nameof(DataMinerInterface), methodName, out stopwatch, nameOfObject);
		}

		private static void Log(Helpers helpers, MethodBase methodBase, string message)
		{
			var methodName = ((WrappedMethodAttribute)methodBase.GetCustomAttributes(typeof(WrappedMethodAttribute), true)[0]).ToString();

			helpers.Log(nameof(DataMinerInterface), methodName, message);
		}

		private static void LogMethodCompleted(Helpers helpers, MethodBase methodBase, Stopwatch stopwatch)
		{
			var methodName = ((WrappedMethodAttribute)methodBase.GetCustomAttributes(typeof(WrappedMethodAttribute), true)[0]).ToString();

			helpers.LogMethodCompleted(nameof(DataMinerInterface), methodName, null, stopwatch);
		}
	}
}
