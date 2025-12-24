namespace Library.Utilities.EqualityComparers
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
    using System.Collections.Generic;

    public  class EventByIdEqualityComparer : IEqualityComparer<Event>
    {
        public bool Equals(Event x, Event y)
        {
            return Equals(x?.Id, y?.Id);
        }

        public int GetHashCode(Event obj)
        {
            if (obj == null) return -1;
            return obj.Id.GetHashCode();
        }
    }
}
