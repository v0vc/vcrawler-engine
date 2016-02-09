// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class EnumerableExtensions
    {
        #region Static Methods

        /// <summary>
        ///     Splits provided enumerable into batches of the specified size
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            int count = 0;

            foreach (TSource item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;
                if (count != size)
                {
                    continue;
                }

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
            {
                yield return bucket.Take(count);
            }
        }

        public static bool CollectionsEquals(this IEnumerable<string> collection, IEnumerable<string> collectionToCompare)
        {
            IOrderedEnumerable<string> firstOrdered = collection.OrderBy(x => x.ToString());

            IOrderedEnumerable<string> secondOrdered = collectionToCompare.OrderBy(x => x.ToString());

            return firstOrdered.SequenceEqual(secondOrdered, StringComparer.OrdinalIgnoreCase);
        }

        public static void ForEach<TItem>(this IEnumerable<TItem> sequence, Action<TItem> action)
        {
            foreach (TItem obj in sequence)
            {
                action(obj);
            }
        }

        public static bool ListsContentdEquals(this IReadOnlyCollection<string> list1, IReadOnlyCollection<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            var ids = new HashSet<string>(list2);
            return list1.All(ids.Contains);
        }

        public static IEnumerable<List<string>> SplitList(this List<string> locations, int nSize = 50)
        {
            var list = new List<List<string>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        #endregion
    }
}
