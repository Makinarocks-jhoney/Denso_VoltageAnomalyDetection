using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace MVP_Voltage.Converter
{
    public sealed class RoiHandlePosConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Length < 5) return 0.0;
            if (parameter is not string p) return 0.0;

            double x = ToDouble(values[0]);
            double y = ToDouble(values[1]);
            double w = ToDouble(values[2]);
            double h = ToDouble(values[3]);
            double s = ToDouble(values[4]);
            double hs = s / 2.0;

            // 기준점(ROI)
            double left = x;
            double top = y;
            double right = x + w;
            double bottom = y + h;
            double cx = x + w / 2.0;
            double cy = y + h / 2.0;

            // 예: "NW.Left"
            var parts = p.Split('.');
            if (parts.Length != 2) return 0.0;
            var handle = parts[0];
            var axis = parts[1];

            double px = 0, py = 0;

            switch (handle)
            {
                case "N": px = cx; py = top; break;
                case "S": px = cx; py = bottom; break;
                case "W": px = left; py = cy; break;
                case "E": px = right; py = cy; break;
                case "NW": px = left; py = top; break;
                case "NE": px = right; py = top; break;
                case "SW": px = left; py = bottom; break;
                case "SE": px = right; py = bottom; break;
                default: px = left; py = top; break;
            }

            // Thumb의 좌상단 좌표로 변환(정중앙 정렬)
            if (axis == "Left") return px - hs;
            if (axis == "Top") return py - hs;

            return 0.0;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

        private static double ToDouble(object v)
        {
            if (v is null) return 0.0;
            if (v is double d) return d;
            if (v is float f) return f;
            if (v is int i) return i;
            if (v is long l) return l;
            if (double.TryParse(v.ToString(), out var r)) return r;
            return 0.0;
        }
    }
}
