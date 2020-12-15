using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Transactions
{
    public enum TransactionCurrency : int
    {
        Bitcoin = 0,
        Monero = 1
    }

    public static class TransactionCurrencyExtensions
    {
        /// <summary>
        /// Retrieves the number of confirmations a transaction should have to be deemed confirmed.
        /// </summary>
        public static int GetDefaultConfirmationCount(this TransactionCurrency currency)
        {
            switch (currency)
            {
                case TransactionCurrency.Bitcoin:
                    return 1;
                case TransactionCurrency.Monero:
                    return 5;
            }

            throw new ArgumentException("No data for default confirmation threshold for unknown currency", nameof(currency));
        }

        /// <summary>
        /// Retrieves a three- or four-letter currency code (ex. "BTC", "XMR") for a given cryptocurrency.
        /// </summary>
        public static string GetCurrencyCode(this TransactionCurrency currency)
        {
            switch (currency)
            {
                case TransactionCurrency.Bitcoin:
                    return "BTC";
                case TransactionCurrency.Monero:
                    return "XMR";
            }

            throw new ArgumentException("No currency code for unknown currency", nameof(currency));
        }

        /// <summary>
        /// Formats a transaction value for pretty printing (i.e. 470000 satoshis -> "0.00470000 BTC")
        /// </summary>
        /// <param name="currency">The currency in which the transaction is conducted.</param>
        /// <param name="value">The transaction value.</param>
        /// <returns>A pretty-printed string in the format of "[transaction value] [currency code]"</returns>
        public static string PrettyPrintedValue(this TransactionCurrency currency, long value)
        {
            long decimalPlaces = 0;

            switch (currency)
            {
                case TransactionCurrency.Bitcoin:
                    decimalPlaces = 8;
                    break;
                case TransactionCurrency.Monero:
                    decimalPlaces = 12;
                    break;
                default:
                    throw new ArgumentException("No pretty printing information for unknown currency", nameof(currency));
            }

            long divisor = 1;
            for (int i = 0; i < decimalPlaces; ++i)
            {
                divisor *= 10;
            }

            return $"{value / divisor}.{(value % divisor).ToString($"D{decimalPlaces}")} {currency.GetCurrencyCode()}";
        }
    }
}
