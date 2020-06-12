using System.Globalization;

namespace Excubo.Blazor.Diagrams.Extensions
{
    internal static class ToStringExtension
    {
        public static string ToNeutralString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static string ToNeutralString(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
