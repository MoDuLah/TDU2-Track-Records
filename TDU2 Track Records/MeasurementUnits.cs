using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    internal static class MeasurementUnits
    {
        public const double MilesPerKilometer = 0.621371192;
        public const double PoundsPerKilogram = 2.20462262;
        public const double PoundFeetPerNewtonMeter = 0.737562149;

        public static bool IsImperial => Settings.Default.system == "Imperial";

        private sealed class DraftConversion
        {
            public string OriginalText;
            public bool IsImperial;
        }

        private static readonly ConditionalWeakTable<TextBox, DraftConversion> DraftConversions =
            new ConditionalWeakTable<TextBox, DraftConversion>();

        public static bool TryParse(string text, out double value)
        {
            return (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                    double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) &&
                   !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static double FromMetric(double value, double factor) => IsImperial ? value * factor : value;

        public static double ToMetric(double value, double factor) => IsImperial ? value / factor : value;

        public static double DraftToMetric(TextBox field, double value, double factor)
        {
            if (IsImperial && DraftConversions.TryGetValue(field, out DraftConversion previous) &&
                previous.IsImperial && TryParse(previous.OriginalText, out double originalMetric))
                return originalMetric;

            return ToMetric(value, factor);
        }

        public static string ConvertText(string text, double factor, bool toImperial, string format = "0.##")
        {
            if (!TryParse(text, out double value))
                return text;

            double converted = toImperial ? value * factor : value / factor;
            return converted.ToString(format, CultureInfo.CurrentCulture);
        }

        public static void ConvertDraft(TextBox source, double factor, bool toImperial, TextBox destination = null)
        {
            destination = destination ?? source;
            string text = source.Text;
            // Reuse the exact prior input if the rounded display value has not been edited.
            string converted = DraftConversions.TryGetValue(source, out DraftConversion previous) &&
                               previous.IsImperial != toImperial
                ? previous.OriginalText
                : ConvertText(text, factor, toImperial);

            ClearDraftConversion(destination, null);
            destination.Text = converted;
            if (TryParse(text, out _))
            {
                DraftConversions.Add(destination, new DraftConversion { OriginalText = text, IsImperial = toImperial });
                destination.TextChanged += ClearDraftConversion;
            }
        }

        private static void ClearDraftConversion(object sender, TextChangedEventArgs e)
        {
            var field = (TextBox)sender;
            field.TextChanged -= ClearDraftConversion;
            DraftConversions.Remove(field);
        }

        public static bool TryReadMetricInteger(string text, double factor, out int result)
        {
            result = 0;
            if (!TryParse(text, out double value) || value < 0)
                return false;

            double metric = Math.Round(ToMetric(value, factor));
            if (metric < 0 || metric > int.MaxValue)
                return false;

            result = (int)metric;
            return true;
        }
    }
}
