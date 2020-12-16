using System;
using System.Globalization;

namespace Excubo.Blazor.Diagrams.Extensions
{
    public sealed class CultureSwapper : IDisposable
    {
        private readonly CultureInfo old_culture;
        /// <summary>
        /// Create a temporary environment with a specific culture with a using statement:
        /// 
        /// using (var temp_env = new CultureSwapper(new CultureInfo("de-DE")))
        /// {
        /// }
        /// 
        /// The culture is reset to the original culture after the end of the using block.
        /// 
        /// new CultureSwapper() is equivalent with new CultureSwapper(CultureInfo.InvariantCulture)).
        /// </summary>
        /// <param name="new_culture"></param>
        public CultureSwapper(CultureInfo new_culture = null)
        {
            old_culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new_culture ?? CultureInfo.InvariantCulture;
        }
        public CultureSwapper Suspend()
        {
            return new CultureSwapper(old_culture);
        }
        public void Dispose()
        {
            CultureInfo.CurrentCulture = old_culture;
        }
    }
}