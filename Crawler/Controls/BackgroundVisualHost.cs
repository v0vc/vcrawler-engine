// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved
// http://gettinggui.com/creating-a-busy-indicator-in-a-separate-thread-in-wpf/

using System;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Crawler.Controls
{
    public delegate Visual CreateContentFunction();

    public class BackgroundVisualHost : FrameworkElement
    {
        #region Static and Readonly Fields

        /// <summary>
        ///     Identifies the CreateContent dependency property.
        /// </summary>
        public static readonly DependencyProperty CreateContentProperty = DependencyProperty.Register("CreateContent", 
            typeof(CreateContentFunction), 
            typeof(BackgroundVisualHost), 
            new FrameworkPropertyMetadata(OnCreateContentChanged));

        /// <summary>
        ///     Identifies the IsContentShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentShowingProperty = DependencyProperty.Register("IsContentShowing", 
            typeof(bool), 
            typeof(BackgroundVisualHost), 
            new FrameworkPropertyMetadata(false, OnIsContentShowingChanged));

        #endregion

        #region Fields

        private HostVisual hostVisual = null;
        private ThreadedVisualHelper threadedHelper = null;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the function used to create the visual to display in a background thread.
        /// </summary>
        public CreateContentFunction CreateContent
        {
            get
            {
                return (CreateContentFunction)GetValue(CreateContentProperty);
            }
            set
            {
                SetValue(CreateContentProperty, value);
            }
        }

        /// <summary>
        ///     Gets or sets if the content is being displayed.
        /// </summary>
        public bool IsContentShowing
        {
            get
            {
                return (bool)GetValue(IsContentShowingProperty);
            }
            set
            {
                SetValue(IsContentShowingProperty, value);
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (hostVisual != null)
                {
                    yield return hostVisual;
                }
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return hostVisual != null ? 1 : 0;
            }
        }

        #endregion

        #region Static Methods

        private static void OnCreateContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bvh = (BackgroundVisualHost)d;

            if (bvh.IsContentShowing)
            {
                bvh.HideContentHelper();
                if (e.NewValue != null)
                {
                    bvh.CreateContentHelper();
                }
            }
        }

        private static void OnIsContentShowingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bvh = (BackgroundVisualHost)d;

            if (bvh.CreateContent != null)
            {
                if ((bool)e.NewValue)
                {
                    bvh.CreateContentHelper();
                }
                else
                {
                    bvh.HideContentHelper();
                }
            }
        }

        #endregion

        #region Methods

        protected override Visual GetVisualChild(int index)
        {
            if (hostVisual != null && index == 0)
            {
                return hostVisual;
            }

            throw new IndexOutOfRangeException("index");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (threadedHelper != null)
            {
                return threadedHelper.DesiredSize;
            }

            return base.MeasureOverride(availableSize);
        }

        private void CreateContentHelper()
        {
            threadedHelper = new ThreadedVisualHelper(CreateContent, SafeInvalidateMeasure);
            hostVisual = threadedHelper.OstVisual;
        }

        private void HideContentHelper()
        {
            if (threadedHelper != null)
            {
                threadedHelper.Exit();
                threadedHelper = null;
                InvalidateMeasure();
            }
        }

        private void SafeInvalidateMeasure()
        {
            Dispatcher.BeginInvoke(new Action(InvalidateMeasure), DispatcherPriority.Loaded);
        }

        #endregion

        #region Nested type: ThreadedVisualHelper

        private class ThreadedVisualHelper
        {
            #region Static and Readonly Fields

            private readonly CreateContentFunction createContent;
            private readonly Action invalidateMeasure;
            private readonly HostVisual ostVisual = null;
            private readonly AutoResetEvent sync = new AutoResetEvent(false);

            #endregion

            #region Constructors

            public ThreadedVisualHelper(CreateContentFunction createContent, Action invalidateMeasure)
            {
                ostVisual = new HostVisual();
                this.createContent = createContent;
                this.invalidateMeasure = invalidateMeasure;

                var backgroundUi = new Thread(CreateAndShowContent);
                backgroundUi.SetApartmentState(ApartmentState.STA);
                backgroundUi.Name = "BackgroundVisualHostThread";
                backgroundUi.IsBackground = true;
                backgroundUi.Start();

                sync.WaitOne();
            }

            #endregion

            #region Properties

            public Size DesiredSize { get; private set; }

            public HostVisual OstVisual
            {
                get
                {
                    return ostVisual;
                }
            }

            private Dispatcher Dispatcher { get; set; }

            #endregion

            #region Methods

            public void Exit()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }

            private void CreateAndShowContent()
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
                var source = new VisualTargetPresentationSource(ostVisual);
                sync.Set();
                source.RootVisual = createContent();
                DesiredSize = source.DesiredSize;
                invalidateMeasure();

                Dispatcher.Run();
                source.Dispose();
            }

            #endregion
        }

        #endregion
    }
}
