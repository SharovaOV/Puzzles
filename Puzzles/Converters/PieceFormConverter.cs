using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Converters;
using Puzzles.Models;

namespace Puzzles.Converters
{
    public sealed class PieceFormConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {

                try
                {
                    return PieceConfig.ParsePieceForm(s);
                }
                catch (NotImplementedException)
                {
                    throw new FormatException("Invalid edge type value.");
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
