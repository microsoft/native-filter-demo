using System;
using System.Globalization;
using System.Windows.Data;

namespace NativeFilterDemo
{
    public class AspectRatioValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double length = (double)value;
                return length * 3 / 4; //Fixed 4:3 aspect ratio
            }

            throw new NotSupportedException();
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

}
