namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ChangeOrderTimingFailedException : MediaServicesException
    {
        public ChangeOrderTimingFailedException()
        {
        }

        public ChangeOrderTimingFailedException(string orderName)
            : base($"Unable to change timing for order {orderName}")
        {
        }

        public ChangeOrderTimingFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
