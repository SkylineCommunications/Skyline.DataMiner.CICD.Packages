namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class UnexpectedTypeException : MediaServicesException
	{
		public UnexpectedTypeException()
		{
		}

		public UnexpectedTypeException(string message) : base(message)
		{
		}

		public UnexpectedTypeException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
