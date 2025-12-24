namespace RemoveOldServices_1
{
	using Skyline.DataMiner.Automation;

	public static class IEngineExtensions
	{
		public static void Log(this IEngine engine, string className, string methodName, string message)
		{
			engine.Log($"{className}|{methodName}|{message}");
		}
	}
}