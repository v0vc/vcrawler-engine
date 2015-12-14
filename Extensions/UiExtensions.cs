// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Extensions
{
    public static class UiExtensions
    {
        #region Static Methods

        public static ScrollViewer GetScrollbar(DependencyObject dep)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dep, i);
                if (child is ScrollViewer)
                {
                    return child as ScrollViewer;
                }
                ScrollViewer sub = GetScrollbar(child);
                if (sub != null)
                {
                    return sub;
                }
            }
            return null;
        }

        #endregion
    }
}
