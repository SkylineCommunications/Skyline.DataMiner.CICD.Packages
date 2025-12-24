namespace ExecuteFeenixStopCommand_2
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class FeenixElementException : Exception
	{
		public FeenixElementException(string message) : base(message)
		{
		}

		public FeenixElementException() : base("An error occurred with the Feenix Element")
		{
		}

		public FeenixElementException(string message, Exception inner) : base(message, inner)
		{
		}

		protected FeenixElementException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}