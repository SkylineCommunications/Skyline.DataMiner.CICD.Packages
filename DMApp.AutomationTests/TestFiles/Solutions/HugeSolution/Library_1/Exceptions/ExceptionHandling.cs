namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	using Skyline.DataMiner.Automation;

	public static class ExceptionHandling
	{
		public static void LogException(IEngine engine, string methodName, string details, Exception e)
		{
			engine.Log($"{methodName}|{details}: {e.Message}\n{e.ToString()}");
		}
	}
}