// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Controls;
using Crawler.ViewModels;
using Interfaces.Models;

namespace Crawler.Common
{
    public class RelatedStyleSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate SimpleTemplate { private get; set; }
        public DataTemplate StateTemplate { private get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var channel = item as IChannel;
            if (channel != null)
            {
                return channel is StateChannel ? StateTemplate : SimpleTemplate;
            }
            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
