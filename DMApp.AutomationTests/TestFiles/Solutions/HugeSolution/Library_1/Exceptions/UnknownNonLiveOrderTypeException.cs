using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class UnknownNonLiveOrderTypeException : MediaServicesException
	{
		public UnknownNonLiveOrderTypeException()
		{
		}

		public UnknownNonLiveOrderTypeException(int dataminerId, int ticketId)
			: base($"Unknown Non-Live Order type for ticket {dataminerId}/{ticketId}")
		{
		}

        public UnknownNonLiveOrderTypeException(Guid uniqueId)
            : base($"Unknown Non-Live Order type for ticket with unique id {uniqueId}")
        {
        }

        public UnknownNonLiveOrderTypeException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}