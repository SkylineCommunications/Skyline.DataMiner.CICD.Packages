namespace Skyline.DataMiner.CICD.Parsers.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A collection of static helper methods that don't fit anywhere else.
    /// </summary>
    internal static class Tools
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, params object[] args)
        {
            if (!condition && Debugger.IsAttached)
            {
                Trace.WriteLine("assert");
            }

            Debug.Assert(condition, String.Format(message, args));
        }

        /// <summary>
        /// Performs a binary search on a sorted list.
        /// </summary>
        /// <param name="list">A sorted IList.</param>
        /// <param name="eval">An evaluation to indicate the next search direction: -1 to look in lower indices, 1 to look in higher indices, 0 when the object is found.</param>
        /// <returns>The list index containing the requested object. If the list is empty, -1 is returned. If the list contains only one item, 0 is returned without evaluating.</returns>
        public static int BinarySearch<T>(IReadOnlyList<T> list, Func<T, int> eval)
        {
            // Handle invalid arguments
            if (eval == null || list == null || list.Count == 0)
            {
                return -1;
            }

            int first = 0;
            int last = list.Count - 1;

            while (first < last)
            {
                int i = first + ((last - first) / 2);
                int direction = eval(list[i]);

                if (direction == 0)
                {
                    return i;
                }

                if (direction < 0)
                {
                    last = i - 1;
                }
                else
                {
                    first = i + 1;
                }
            }

            return first;
        }
    }
}