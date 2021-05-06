using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Semio.Core.Extensions
{
    /// <summary>
    /// Container for extension methods related to collections
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// returns intersect if both collection contain values otherwise return collection as whole
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static IEnumerable<T> IntersectIfNotEmpty<T>(this IEnumerable<T> collection, List<T> other)
        {
            if (other.Any() && collection.Any())
            {
                return collection.Intersect(other);
            }
            if (other.Any())
            {
                return other;
            }
           return collection;
            

        }

        /// <summary>
        /// Determines whether the items contained in two collections are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The primary collection.</param>
        /// <param name="other">The other collection.</param>
        /// <returns>True if the collections are equal, otherwise false.</returns>
        public static bool CollectionEquals<T>(this IEnumerable<T> collection, IEnumerable<T> other)
        {
            if (collection == null && other == null)
                return true;

            if (collection != null && other != null)
            {
                if (other.Count() != collection.Count())
                    return false;

                for (int i = 0; i < collection.Count(); i++)
                {
                    if (!Equals(collection.ElementAt(i), other.ElementAt(i)))
                        return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a generic list containing the objects in the specified IEnumerable collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The source collection.</param>
        /// <returns></returns>
        public static List<T> ToList<T>(this IEnumerable collection)
        {
            var list = new List<T>();
            foreach (var item in collection)
            {
                if (item is T)
                    list.Add((T)item);
            }
            return list;
        }

        /// <summary>
        /// Determines whether the specified collection is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>
        /// 	<c>true</c> if the specified collection is empty; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsEmpty<T>(this IEnumerable<T> collection) => collection.Count() == 0;

        /// <summary>
        /// Determines whether the specified collection is empty.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>
        /// 	<c>true</c> if the specified collection is empty; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsEmpty(this IEnumerable collection) => IsEmpty<Object>(collection.Cast<Object>());

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public static IList<T> AddRange<T>(this IList<T> collection, IEnumerable<T> other)
        {
            if (other == null)
                return collection;
            foreach (var item in other)
            {
                collection.Add(item);
            }
            return collection;
        }

        /// <summary>
        /// Removes a collection of objects from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static IList<T> RemoveAll<T>(this IList<T> collection, IEnumerable<T> other)
        {
            foreach (var item in other.ToArray())
            {
                collection.Remove(item);
            }
            return collection;
        }

        /// <summary>
        /// Removes a collection of objects from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="other"></param>
        /// <param name="comparator"> </param>
        /// <returns></returns>
        public static IList<T> RemoveAll<T>(this IList<T> collection, IEnumerable<T> other, Func<T, T, bool> comparator)
        {
            foreach (var item in other.ToArray())
            {
                var found = collection.FirstOrDefault(item2 =>comparator(item, item2));
                collection.Remove(found);
            }
            return collection;
        }

        /// <summary>
        /// Find the index of the toFind element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list of elements.</param>
        /// <param name="toFind">The element to find.</param>
        /// <param name="areEqual">A custom method to figure out if the objects are equal</param>
        /// <returns>Index of the specified element, or -1</returns>
        public static int IndexOf<T>(this IList<T> list, T toFind, Func<T, T, Boolean> areEqual)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (areEqual(list[i], toFind))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns an IEnumerable of integers between 1 and the specified count.
        /// </summary>
        /// <param name="count">Maximum number in the range</param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int count) => Range(1, count);

        /// <summary>
        /// Returns an IEnumerable of integers between the start and end values.
        /// </summary>
        /// <param name="start">First value in the returned enumerable</param>
        /// <param name="end">Last value in the returned enumerable</param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int start, int end)
        {
            int index = start;
            while (index <= end)
            {
                yield return index;
                index++;
            }
        }

        /// <summary>
        /// Adds a set of elements into an IDictionary, if the item already exists in the source dictionary then the item in the additions is ignored.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="primary">The primary dictionary to add the elements to.</param>
        /// <param name="additions">The items to add to the primary dictionary.</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> primary, IEnumerable<KeyValuePair<TKey, TValue>> additions)
        {
            foreach (var i in additions)
            {
                if (!primary.ContainsKey(i.Key))
                    primary.Add(i);
            }
        }

        /// <summary>
        /// Creates a combined string representation for the specified items.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns>The combined string.</returns>
        public static string ToCombinedString<T>(this IEnumerable<T> items)
        {
            var sb = new StringBuilder();

            int i = 0;
            int lastIndex = items.Count() - 1;

            foreach (var item in items)
            {
                if (i > 0)
                {
                    sb.Append(i == lastIndex ? " and " : ", ");
                }

                sb.Append(item.ToString());

                i++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts items of a string collection which represent integers to a collection of integers,
        ///  optionally including default values for non-integer strings.
        /// </summary>
        /// <param name="strings">The string collection.</param>
        /// <param name="includeNonInts">if set to <c>true</c> include non integer string values.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static IEnumerable<int> AsIntegers(this IEnumerable<string> strings, bool includeNonInts = false, int defaultValue = Int32.MinValue)
        {
            foreach (string stringVar in strings)
            {
                int parsed;
                if (Int32.TryParse(stringVar, out parsed))
                    yield return parsed;
                else if (includeNonInts)
                    yield return defaultValue;
            }
        }
    }
}