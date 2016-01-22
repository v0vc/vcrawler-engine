// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Controls;
using Interfaces.Models;

namespace Crawler.Behaviors
{
    public class SelectingItemAttachedProperty
    {
        #region Static and Readonly Fields

        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached("SelectingItem", 
            typeof(IChannel), 
            typeof(SelectingItemAttachedProperty), 
            new PropertyMetadata(default(IChannel), OnSelectingItemChanged));

        #endregion

        #region Static Methods

        public static IChannel GetSelectingItem(DependencyObject target)
        {
            return (IChannel)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, IChannel value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        private static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null)
            {
                return;
            }

            // Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.SelectedItem, null);
            });

            // Works with .Net 4.0
            // grid.Dispatcher.BeginInvoke((Action)(() =>
            // {
            // grid.UpdateLayout();
            // grid.ScrollIntoView(grid.SelectedItem, null);
            // }));
        }

        #endregion
    }
}
