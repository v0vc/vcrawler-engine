// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Interfaces.Models;

namespace Crawler.Converters
{
    public class MultiParamConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var tuple = new Tuple<IChannel, DataGrid>((IChannel)values[0], (DataGrid)values[1]);
            return tuple;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
