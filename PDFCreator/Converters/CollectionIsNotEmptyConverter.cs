﻿using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace PDFCreator.Converters;

public class CollectionIsNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ICollection collection)
            return collection.Count > 0;

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}