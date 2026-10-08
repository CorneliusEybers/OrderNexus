
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrderNexus.Infrastructure.Persistence.Converters
{
    /// <summary>
    /// Converts monetary decimal values to integer minor units
    /// for precise storage in SQLite.
    /// </summary>
    public static class MoneyConverter
    {
        #region Public Properties

        /// <summary>
        /// Converts two-decimal-place monetary values to and
        /// from their integer representation.
        /// </summary>
        public static readonly ValueConverter<decimal, long> Instance = new ValueConverter<decimal, long>(amount => checked((long)(amount * 100m)), storedAmount => storedAmount / 100m);

        #endregion
    }
}
