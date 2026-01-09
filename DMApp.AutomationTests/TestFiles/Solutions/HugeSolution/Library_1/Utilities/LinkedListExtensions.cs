namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class LinkedListExtensions
	{
        public static IEnumerable<LinkedListNode<T>> ReverseNodes<T>(this LinkedList<T> list)
        {
            var el = list.Last;
            while (el != null)
            {
                yield return el;
                el = el.Previous;
            }
        }

        public static IEnumerable<LinkedListNode<T>> ReverseNodesStartFrom<T>(this LinkedList<T> list, LinkedListNode<T> startNode)
        {
            if (!list.Contains(startNode.Value)) throw new ArgumentException("Node is not part of the list");

            var el = startNode;
            while (el != null)
            {
                yield return el;
                el = el.Previous;
            }
        }

        public static IEnumerable<LinkedListNode<T>> StartFrom<T>(this LinkedList<T> list, LinkedListNode<T> startNode)
        {
            if (!list.Contains(startNode.Value)) throw new ArgumentException("Node is not part of the list");

            var el = startNode;
            while (el != null)
            {
                yield return el;
                el = el.Next;
            }
        }

        public static IEnumerable<LinkedListNode<T>> StartAfter<T>(this LinkedList<T> list, LinkedListNode<T> startNode)
        {
            if (!list.Contains(startNode.Value)) throw new ArgumentException("Node is not part of the list");

            var el = startNode.Next;
            while (el != null)
            {
                yield return el;
                el = el.Next;
            }
        }

        public static int IndexOf<T>(this LinkedList<T> list, T value)
        {
            if (list.Count == 0) return -1;

            int index = 0;
            var item = list.First;
            do
            {
                if (Equals(item.Value, value)) return index;
                index++;
                item = item.Next;
            } 
            while (item != null);

            return -1;
        }
    }
}
