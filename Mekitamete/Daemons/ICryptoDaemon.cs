using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mekitamete.Daemons
{
    public interface ICryptoDaemon
    {
        /// <summary>
        /// Creates a new address for receiving cryptocurrency. The address may be assigned an optional label.
        /// </summary>
        /// <param name="label">The address label; can be null.</param>
        /// <returns>A newly created address.</returns>
        /// <exception cref="CryptoDaemonException"></exception>
        public string CreateNewAddress(string label = null);

        /// <summary>
        /// Retrieves a list of incoming transactions for a given address.
        /// </summary>
        /// <param name="address">Cryptocurrency receiving address.</param>
        /// <param name="minConfirmations">Does not include transactions below this confirmation threshold.</param>
        /// <returns>A list of all incoming transactions for a given address.</returns>
        public List<CryptoTransaction> GetTransactions(string address, int minConfirmations = 0)
        {
            return GetTransactions(new[] { address }, minConfirmations);
        }

        /// <summary>
        /// Retrieves a list of incoming transactions for given addresses.
        /// </summary>
        /// <param name="addresses">Cryptocurrency receiving addresses. If null, all addresses are included.</param>
        /// <param name="minConfirmations">Does not include transactions below this confirmation threshold.</param>
        /// <returns>A list of all incoming transactions for given addresses.</returns>
        /// <remarks>The same transaction ID may occur multiple times in the result. This will happen if a single transaction contains multiple output addresses specified in the <paramref name="addresses"/> parameter.</remarks>
        public List<CryptoTransaction> GetTransactions(IEnumerable<string> addresses = null, int minConfirmations = 0);

        /// <summary>
        /// Retrieves the received (not current!) balance of a given address.
        /// </summary>
        /// <param name="address">Cryptocurrency receiving address.</param>
        /// <param name="minConfirmations">Does not include the balance from transactions below this confirmation threshold.</param>
        /// <returns>Total received balance of a given address.</returns>
        public long GetReceivedBalance(string address, int minConfirmations = 0)
        {
            return GetTransactions(address, minConfirmations).Sum(x => x.ReceivingValue);
        }

        /// <summary>
        /// Retrieves the sum of received (not current!) balance of given addresses.
        /// </summary>
        /// <param name="addresses">Cryptocurrency receiving addresses.</param>
        /// <param name="minConfirmations">Does not include the balance from transactions below this confirmation threshold.</param>
        /// <returns>Total received balance of a given address.</returns>
        public long GetReceivedBalance(IEnumerable<string> addresses, int minConfirmations = 0)
        {
            return GetTransactions(addresses, minConfirmations).Sum(x => x.ReceivingValue);
        }
    }
}
