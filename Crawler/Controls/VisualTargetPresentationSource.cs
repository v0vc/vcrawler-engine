// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved
// http://gettinggui.com/creating-a-busy-indicator-in-a-separate-thread-in-wpf/

using System.Windows;
using System.Windows.Media;

namespace Crawler.Controls
{
    public class VisualTargetPresentationSource : PresentationSource
    {
        #region Static and Readonly Fields

        private readonly VisualTarget visualTarget;

        #endregion

        #region Fields

        private bool isDisposed = false;

        #endregion

        #region Constructors

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            visualTarget = new VisualTarget(hostVisual);
            AddSource();
        }

        #endregion

        #region Properties

        public override bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        public override Visual RootVisual
        {
            get
            {
                return visualTarget.RootVisual;
            }
            set
            {
                Visual oldRoot = visualTarget.RootVisual;

                // Set the root visual of the VisualTarget.  This visual will
                // now be used to visually compose the scene.
                visualTarget.RootVisual = value;

                // Tell the PresentationSource that the root visual has
                // changed.  This kicks off a bunch of stuff like the
                // Loaded event.
                RootChanged(oldRoot, value);

                // Kickoff layout...
                var rootElement = value as UIElement;
                if (rootElement != null)
                {
                    rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    rootElement.Arrange(new Rect(rootElement.DesiredSize));

                    DesiredSize = rootElement.DesiredSize;
                }
                else
                {
                    DesiredSize = new Size(0, 0);
                }
            }
        }

        public Size DesiredSize { get; private set; }

        #endregion

        #region Methods

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return visualTarget;
        }

        internal void Dispose()
        {
            RemoveSource();
            isDisposed = true;
        }

        #endregion
    }
}
