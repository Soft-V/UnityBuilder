using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;
using UnityBuilder.Models.Enums;

namespace UnityBuilder.Converters
{
    public class NodeStateToBrushConverter : IValueConverter
    {
        public static readonly NodeStateToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not NodeState state)
                return Brushes.Gray;

            var key = state switch
            {
                NodeState.Running => "AccentPrimary",
                NodeState.Done => "SuccessColor",
                NodeState.Error => "DangerColor",
                NodeState.Cancelled => "WarningColor",
                _ => "TextMuted"
            };

            if (Application.Current!.Resources.TryGetResource(key, Application.Current.ActualThemeVariant, out var resource))
            {
                if (resource is Color color)
                    return new SolidColorBrush(color);
                if (resource is IBrush brush)
                    return brush;
            }

            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
