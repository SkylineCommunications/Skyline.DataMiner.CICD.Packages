namespace Skyline.DataMiner.CICD.Parsers.Common.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal static class CollectionExtensions
    {
        /// <summary>
        /// Adds the specified items to the specified list.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="list">The list to which the items should be added.</param>
        /// <param name="items">The items to add.</param>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only or fixed size.</exception>
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
        {
            if (items == null)
            {
                // Nothing to add
                return;
            }

            if (list is List<T> l)
            {
                l.AddRange(items);
                return;
            }

            // Fallback method
            foreach (T t in items)
            {
                list.Add(t);
            }
        }

        /// <summary>
        /// Removes the specified number of items at the specified index.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="list">The list to which the items should be added.</param>
        /// <param name="index">The index at which the items should be removed.</param>
        /// <param name="count">The number of items to remove.</param>
        public static void RemoveRange<T>(this IList<T> list, int index, int count)
        {
            if (count == 0)
            {
                // Nothing to remove
                return;
            }

            if (list is List<T> l)
            {
                l.RemoveRange(index, count);
                return;
            }

            // fallback method, very slow for large ranges
            for (int i = 0; i < count; i++)
            {
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Returns the selected items from the specified source.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="source">The source from which to select items.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The selected items.</returns>
        public static IEnumerable<T> Descendants<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var queue = new Queue<T>(source);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                yield return item;

                foreach (var child in selector(item))
                {
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Skips the specified number of elements in the specified list and returns the remaining elements.
        /// </summary>
        /// <typeparam name="TSource">The item type.</typeparam>
        /// <param name="source">An <see cref="IList{TSource}"/> to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An <see cref="IEnumerable{TSource}"/> that contains the elements that occur after the specified index in the input sequence.</returns>
        /// <remarks>Similar method exists in LINQ but is not optimized for lists.</remarks>
        public static IEnumerable<TSource> ListSkip<TSource>(this IList<TSource> source, int count)
        {
            for (int i = count; i < source.Count; i++)
            {
                yield return source[i];
            }
        }

        /// <summary>
        /// Iterates the list in backwards order.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="list">The list to iterate.</param>
        /// <returns>The items in reverse order.</returns>
        public static IEnumerable<T> Backwards<T>(this IList<T> list)
        {
            for (int x = list.Count; --x >= 0;)
            {
                yield return list[x];
            }
        }

        /// <summary>
        /// Replaces the specified number of items at the specified index with the specified items.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="list">The list from which the items should be replaced.</param>
        /// <param name="index">The index from where the replacement should be done.</param>
        /// <param name="count">The number of items to replace.</param>
        /// <param name="newItems">The replacement items.</param>
        public static void ReplaceRange<T>(this IList<T> list, int index, int count, IEnumerable<T> newItems)
        {
            list.RemoveRange(index, count);
            list.InsertRange(index, newItems);
        }

        /// <summary>
        /// Inserts the specified items to the specified list.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="list">The list to which the items should be added.</param>
        /// <param name="index">The index at which the items should be inserted.</param>
        /// <param name="items">The items to add.</param>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only or fixed size.</exception>
        public static void InsertRange<T>(this IList<T> list, int index, IEnumerable<T> items)
        {
            if (items == null)
            {
                // Nothing to add
                return;
            }

            if (list is List<T> l)
            {
                l.InsertRange(index, items);
                return;
            }

            // Fallback method, very slow for large ranges
            foreach (T t in items)
            {
                list.Insert(index++, t);
            }
        }
    }
}