namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class AttachedBehavior
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
