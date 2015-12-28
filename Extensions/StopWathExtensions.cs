// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Diagnostics;

namespace Extensions
{
    public static class StopWathExtensions
    {
        #region Constants

        private const string baseMessage = "Time elapsed:";

        #endregion

        #region Static Methods

        public static string TakeLogMessage(this Stopwatch watch)
        {
            return string.Format(watch.Elapsed.Hours != 0 ? "{0} {1:hh\\:mm\\:ss}" : "{0} {1:mm\\:ss}", baseMessage, watch.Elapsed);
        }

        #endregion
    }
}
