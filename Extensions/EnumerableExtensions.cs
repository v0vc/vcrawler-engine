// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class EnumerableExtensions
    {
        #region Static Methods

        public static void ForEach<TItem>(this IEnumerable<TItem> sequence, Action<TItem> action)
        {
            foreach (TItem obj in sequence)
            {
                action(obj);
            }
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

        #endregion
    }
}
