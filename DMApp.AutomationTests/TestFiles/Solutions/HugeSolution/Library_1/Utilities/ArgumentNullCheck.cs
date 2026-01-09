namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;

	public static class ArgumentNullCheck
	{
		public static void ThrowIfNull(object arg, string argName)
		{
			if (arg == null) throw new ArgumentNullException(argName);
		}

		public static void ThrowIfNullOrWhiteSpace(string arg, string argName)
		{
			if (string.IsNullOrWhiteSpace(arg)) throw new ArgumentNullException(argName);
		}
	}
}
