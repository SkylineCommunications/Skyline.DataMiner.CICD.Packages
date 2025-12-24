using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
    public class StopOrderFailedException : MediaServicesException
    {
        public StopOrderFailedException()
        {
        }

        public StopOrderFailedException(string orderName)
            : base($"Unable to stop order {orderName}")
        {
        }

        public StopOrderFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
