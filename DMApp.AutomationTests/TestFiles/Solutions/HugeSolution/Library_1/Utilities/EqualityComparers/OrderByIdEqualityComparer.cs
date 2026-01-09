namespace Library.Utilities.EqualityComparers
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using System.Collections.Generic;

    public class OrderByIdEqualityComparer : IEqualityComparer<Order>
    {
        public bool Equals(Order x, Order y)
        {
            return Equals(x?.Id, y?.Id);
        }

        public int GetHashCode(Order obj)
        {
            if (obj == null) return -1;
            return obj.Id.GetHashCode();
        }
    }
}
