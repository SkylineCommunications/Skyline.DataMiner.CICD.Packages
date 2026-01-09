namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class ScriptParameterException : Exception
	{
		public ScriptParameterException()
		{
		}

		public ScriptParameterException(string message)
			: base(message)
		{
		}

		public ScriptParameterException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ScriptParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
