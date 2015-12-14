// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Crawler.Behaviors
{
    public static class DataGridBehavior
    {
        #region Static and Readonly Fields

        public static readonly DependencyProperty AutoscrollProperty = DependencyProperty.RegisterAttached("Autoscroll", 
            typeof(bool), 
            typeof(DataGridBehavior), 
            new PropertyMetadata(default(bool), AutoscrollChangedCallback));

        private static readonly Dictionary<DataGrid, NotifyCollectionChangedEventHandler> handlersDict =
            new Dictionary<DataGrid, NotifyCollectionChangedEventHandler>();

        #endregion

        #region Static Methods

        public static bool GetAutoscroll(DependencyObject element)
        {
            return (bool)element.GetValue(AutoscrollProperty);
        }

        public static void SetAutoscroll(DependencyObject element, bool value)
        {
            element.SetValue(AutoscrollProperty, value);
        }

        private static void AutoscrollChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null)
            {
                throw new InvalidOperationException("Dependency object is not DataGrid.");
            }

            if ((bool)args.NewValue)
            {
                Subscribe(dataGrid);
                dataGrid.Unloaded += DataGridOnUnloaded;
                dataGrid.Loaded += DataGridOnLoaded;
            }
            else
            {
                Unsubscribe(dataGrid);
                dataGrid.Unloaded -= DataGridOnUnloaded;
                dataGrid.Loaded -= DataGridOnLoaded;
            }
        }

        private static void DataGridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
            {
                Subscribe(dataGrid);
            }
        }

        private static void DataGridOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
            {
                Unsubscribe(dataGrid);
            }
        }

        // private static void ScrollToEnd(DataGrid datagrid)
        // {
        // if (datagrid.Items.Count == 0)
        // {
        // return;
        // }
        // datagrid.ScrollIntoView(datagrid.Items[datagrid.Items.Count - 1]);
        // }
        private static void ScrollToTop(DataGrid datagrid)
        {
            if (datagrid.Items.Count == 0)
            {
                return;
            }
            datagrid.ScrollIntoView(datagrid.Items[0]);
        }

        private static void Subscribe(DataGrid dataGrid)
        {
            var handler = new NotifyCollectionChangedEventHandler((sender, eventArgs) => ScrollToTop(dataGrid));
            handlersDict.Add(dataGrid, handler);
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += handler;
            ScrollToTop(dataGrid);
        }

        private static void Unsubscribe(DataGrid dataGrid)
        {
            NotifyCollectionChangedEventHandler handler;
            handlersDict.TryGetValue(dataGrid, out handler);
            if (handler == null)
            {
                return;
            }
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged -= handler;
            handlersDict.Remove(dataGrid);
        }

        #endregion
    }
}
