using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ProgrammerLifeSimulator.Converters 
{
    public class RarityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var rarity = value?.ToString() ?? "Common";
            return rarity switch
            {
                "Rare" => Brushes.Cyan,        // 稀有：青色
                "Epic" => Brushes.Purple,      // 史诗：紫色
                "Legendary" => Brushes.Gold,   // 传说：金色
                _ => Brushes.Gray              // 普通：灰色
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
    
    public class StressToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int stress)
            {
                if (stress >= 80) return Brushes.Red;    // 压力过大变红
                if (stress >= 50) return Brushes.Orange; // 压力较大变橙
                return Brushes.Green;                    // 压力正常变绿
            }
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}