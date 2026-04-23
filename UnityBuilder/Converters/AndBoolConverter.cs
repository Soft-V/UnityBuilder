using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBuilder.Converters
{
    public class AndBoolConverter : IMultiValueConverter
    {
        public static readonly AndBoolConverter Instance = new();

        public object Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var v in values)
            {
                if (v is bool b && !b)
                    return false;
            }
            return true;
        }
    }
}
