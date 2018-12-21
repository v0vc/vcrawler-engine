// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Crawler.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        #region Static and Readonly Fields

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused",
            typeof(bool),
            typeof(TextBoxBehavior),
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
            var uie = (TextBox)d;
            if (!(bool)e.NewValue)
            {
                return;
            }

            var action = new Action(() => uie.Dispatcher.BeginInvoke((Action)(() =>
            {
                uie.Focus();
                uie.SelectAll();
            })));
            Task.Factory.StartNew(action);
        }

        #endregion
    }
}
