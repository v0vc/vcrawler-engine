// This file contains my intellectual property. Release of this file requires prior approval from me.
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

        #endregion
    }
}
