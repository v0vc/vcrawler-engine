// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Crawler.Controls
{
    internal class CustomDataGrid : DataGrid
    {
        #region Static and Readonly Fields

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register("SelectedItemsList",
            typeof(IList),
            typeof(CustomDataGrid),
            new PropertyMetadata(null));

        #endregion

        #region Constructors

        public CustomDataGrid()
        {
            SelectionChanged += CustomDataGridSelectionChanged;
        }

        #endregion

        #region Properties

        public IList SelectedItemsList
        {
            get
            {
                return (IList)GetValue(SelectedItemsListProperty);
            }
            set
            {
                SetValue(SelectedItemsListProperty, value);
            }
        }

        #endregion

        #region Event Handling

        private void CustomDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        #endregion
    }
}
