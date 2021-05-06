using System;
using System.Collections.Generic;
using System.Linq;

namespace EnrollmentAlgorithm.Methods
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the default type value if element does not exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <param name="default">The default.</param>
        /// <returns>T.</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static T ElementAtOrDefault<T>(this IList<T> list, int index,Func<T> @default)
        {
            return index >= 0 && index < list.Count ? list[index] : @default();
        }

        /// <summary>
        /// Returns the default value for the specified type if the key does not exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret;
            // Ignore return value
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        /// <summary>
        /// Returns the value at a specific key for a dictionary-type object.  If the key doesn't exist, a user-defined function is executed to return the value.  
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="dictionary">The dictionary-type object.</param>
        /// <param name="key">The key.</param>
        /// <param name="default">The user supplied function to executed if the key isn't found.</param>
        /// <returns>TValue.</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> @default )
        {
            TValue ret;      
            return dictionary.TryGetValue(key, out ret) ? ret : @default();
        }

        //public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) 
        //{
        //    if (dictionary == null)
        //        throw new ArgumentNullException(nameof(dictionary));

        //    foreach (var pair in dictionary)
        //        if (value.Equals(pair.Value)) return pair.Key;

        //    throw new Exception("the value is not found in the dictionary");
        //}

        public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) where TValue : IComparable
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            foreach (var pair in dictionary)
                //if (value.Equals(pair.Value)) return pair.Key;
                if (pair.Value.CompareTo(value) >= 0) return pair.Key;

            throw new Exception("the value is not found in the dictionary");
        }


        /// <summary>
        /// Averages the specified longs.  Helper extension method for <see cref="AverageDate" />.  This is to avoid
        /// overflowing the long data type typically used when averaging Ticks (how date averages are typically calculated).  See
        /// http://stackoverflow.com/questions/10149497/how-can-i-average-a-datetime-field-with-a-linq-query for additional details.
        /// </summary>
        /// <param name="longs">The longs.</param>
        /// <returns>System.Int64.</returns>
        private static long Average(this IEnumerable<long> longs)
        {
            var enumerable = longs as IList<long> ?? longs.ToList();
            long count = enumerable.Count();
            return enumerable.Sum(val => val/count);
        }

        /// <summary>
        /// Generates the average date when given a list of DateTime values.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <returns>DateTime.</returns>
        public static DateTime AverageDate(this IEnumerable<DateTime> dates)
        {
            return new DateTime(dates.Select(x => x.Ticks).Average());
        }
    }
}
