// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Crawler.Behaviors
{
    public class RoutedEventTrigger : EventTriggerBase<DependencyObject>
    {
        #region Constructors

        public RoutedEventTrigger()
        {
        }

        #endregion

        #region Properties

        public RoutedEvent RoutedEvent { get; set; }

        #endregion

        #region Methods

        protected override string GetEventName()
        {
            return RoutedEvent.Name;
        }

        protected override void OnAttached()
        {
            var behavior = AssociatedObject as Behavior;
            var associatedElement = AssociatedObject as FrameworkElement;

            if (behavior != null)
            {
                associatedElement = ((IAttachedObject)behavior).AssociatedObject as FrameworkElement;
            }
            if (associatedElement == null)
            {
                throw new ArgumentException("Routed Event trigger can only be associated to framework elements");
            }
            if (RoutedEvent != null)
            {
                associatedElement.AddHandler(RoutedEvent, new RoutedEventHandler(OnRoutedEvent));
            }
        }

        #endregion

        #region Event Handling

        private void OnRoutedEvent(object sender, RoutedEventArgs args)
        {
            OnEvent(args);
        }

        #endregion
    }
}
