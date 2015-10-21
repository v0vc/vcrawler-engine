// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved
// http://gettinggui.com/creating-a-busy-indicator-in-a-separate-thread-in-wpf/

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Crawler.Controls
{
    [StyleTypedProperty(Property = "BusyStyle", StyleTargetType = typeof(Control))]
    public class BusyDecorator : Decorator
    {
        #region Static and Readonly Fields

        /// <summary>
        ///     Identifies the <see cref="BusyHorizontalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyHorizontalAlignmentProperty = DependencyProperty.Register(
                                                                                                                "BusyHorizontalAlignment", 
            typeof(HorizontalAlignment), 
            typeof(BusyDecorator), 
            new FrameworkPropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        ///     Identifies the <see cref="BusyStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyStyleProperty = DependencyProperty.Register("BusyStyle", 
            typeof(Style), 
            typeof(BusyDecorator), 
            new FrameworkPropertyMetadata(OnBusyStyleChanged));

        /// <summary>
        ///     Identifies the <see cref="BusyVerticalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyVerticalAlignmentProperty = DependencyProperty.Register("BusyVerticalAlignment", 
            typeof(VerticalAlignment), 
            typeof(BusyDecorator), 
            new FrameworkPropertyMetadata(VerticalAlignment.Center));

        /// <summary>
        ///     Identifies the IsBusyIndicatorShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyIndicatorShowingProperty = DependencyProperty.Register("IsBusyIndicatorShowing", 
            typeof(bool), 
            typeof(BusyDecorator), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private readonly BackgroundVisualHost busyHost = new BackgroundVisualHost();

        #endregion

        #region Constructors

        static BusyDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyDecorator), new FrameworkPropertyMetadata(typeof(BusyDecorator)));
        }

        public BusyDecorator()
        {
            AddLogicalChild(busyHost);
            AddVisualChild(busyHost);

            SetBinding(busyHost, IsBusyIndicatorShowingProperty, BackgroundVisualHost.IsContentShowingProperty);
            SetBinding(busyHost, BusyHorizontalAlignmentProperty, HorizontalAlignmentProperty);
            SetBinding(busyHost, BusyVerticalAlignmentProperty, VerticalAlignmentProperty);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the HorizontalAlignment to use to layout the control that contains the busy indicator control.
        /// </summary>
        public HorizontalAlignment BusyHorizontalAlignment
        {
            get
            {
                return (HorizontalAlignment)GetValue(BusyHorizontalAlignmentProperty);
            }
            set
            {
                SetValue(BusyHorizontalAlignmentProperty, value);
            }
        }

        /// <summary>
        ///     Gets or sets the Style to apply to the Control that is displayed as the busy indication.
        /// </summary>
        public Style BusyStyle
        {
            get
            {
                return (Style)GetValue(BusyStyleProperty);
            }
            set
            {
                SetValue(BusyStyleProperty, value);
            }
        }

        /// <summary>
        ///     Gets or sets the the VerticalAlignment to use to layout the control that contains the busy indicator.
        /// </summary>
        public VerticalAlignment BusyVerticalAlignment
        {
            get
            {
                return (VerticalAlignment)GetValue(BusyVerticalAlignmentProperty);
            }
            set
            {
                SetValue(BusyVerticalAlignmentProperty, value);
            }
        }

        /// <summary>
        ///     Gets or sets if the BusyIndicator is being shown.
        /// </summary>
        public bool IsBusyIndicatorShowing
        {
            get
            {
                return (bool)GetValue(IsBusyIndicatorShowingProperty);
            }
            set
            {
                SetValue(IsBusyIndicatorShowingProperty, value);
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (Child != null)
                {
                    yield return Child;
                }

                yield return busyHost;
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return Child != null ? 2 : 1;
            }
        }

        #endregion

        #region Static Methods

        private static void OnBusyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bd = (BusyDecorator)d;
            var nVal = (Style)e.NewValue;
            bd.busyHost.CreateContent = () => new Control { Style = nVal };
        }

        #endregion

        #region Methods

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var ret = new Size(0, 0);
            if (Child != null)
            {
                Child.Arrange(new Rect(arrangeSize));
                ret = Child.RenderSize;
            }

            busyHost.Arrange(new Rect(arrangeSize));

            return new Size(Math.Max(ret.Width, busyHost.RenderSize.Width), Math.Max(ret.Height, busyHost.RenderSize.Height));
        }

        protected override Visual GetVisualChild(int index)
        {
            if (Child != null)
            {
                switch (index)
                {
                    case 0:
                        return Child;

                    case 1:
                        return busyHost;
                }
            }
            else if (index == 0)
            {
                return busyHost;
            }

            throw new IndexOutOfRangeException("index");
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var ret = new Size(0, 0);
            if (Child != null)
            {
                Child.Measure(constraint);
                ret = Child.DesiredSize;
            }

            busyHost.Measure(constraint);

            return new Size(Math.Max(ret.Width, busyHost.DesiredSize.Width), Math.Max(ret.Height, busyHost.DesiredSize.Height));
        }

        private void SetBinding(DependencyObject obj, DependencyProperty source, DependencyProperty target)
        {
            var b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath(source);
            BindingOperations.SetBinding(obj, target, b);
        }

        #endregion
    }
}
