using System.Globalization;

namespace Fume.Web.Helpers
{
    public static class FormatHelper
    {
        private static readonly CultureInfo DominikanCulture = new CultureInfo("es-DO")
        {
            NumberFormat = new NumberFormatInfo
            {
                CurrencySymbol = "RD$",
                CurrencyDecimalSeparator = ".",
                CurrencyGroupSeparator = ",",
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ",",
                CurrencyDecimalDigits = 2,
                CurrencyNegativePattern = 1
            }
        };

        private static readonly CultureInfo InvariantPriceCulture = new CultureInfo("en-US");

        /// <summary>
        /// Formatea un precio en moneda Dominicana (RD$)
        /// </summary>
        /// <param name="price">Precio a formatear</param>
        /// <returns>Precio formateado como "RD$4,500.00"</returns>
        public static string FormatPrice(decimal price)
        {
            return $"RD${price.ToString("N2", InvariantPriceCulture)}";
        }

        /// <summary>
        /// Formatea un precio en moneda Dominicana (RD$) con símbolo personalizado
        /// </summary>
        /// <param name="price">Precio a formatear</param>
        /// <returns>Precio formateado como "RD$ 4,500.00"</returns>
        public static string FormatPriceWithSpace(decimal price)
        {
            return $"RD$ {price.ToString("N2", InvariantPriceCulture)}";
        }

        /// <summary>
        /// Formatea un precio usando la cultura Dominicana completa
        /// </summary>
        /// <param name="price">Precio a formatear</param>
        /// <returns>Precio formateado como "RD$4,500.00"</returns>
        public static string FormatPriceDominican(decimal price)
        {
            return price.ToString("C2", DominikanCulture);
        }
    }
}
