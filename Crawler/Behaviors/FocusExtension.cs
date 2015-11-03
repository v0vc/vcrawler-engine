// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using System.Windows;

namespace Crawler.Behaviors
{
    public static class FocusExtension
    {
        #region Static and Readonly Fields

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", 
            typeof(bool), 
            typeof(FocusExtension), 
            new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        #endregion

        #region Static Methods

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if (!(bool)e.NewValue)
            {
                return;
            }

            // uie.UpdateLayout();
            // uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            var action = new Action(() => uie.Dispatcher.BeginInvoke((Action)(() => uie.Focus())));
            Task.Factory.StartNew(action);
        }

        #endregion
    }
}
