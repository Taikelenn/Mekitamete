using System;
using System.Collections.Generic;
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
        public string CreateNewAddress(string label);

        /// <summary>
        /// Returns the received (not current!) balance of a given address.
        /// </summary>
        /// <param name="address">Cryptocurrency receiving address.</param>
        /// <param name="minConfirmations">Does not include the balance from transactions below this confirmation threshold. A value of zero allows the method to include all transactions, including unconfirmed ones.</param>
        /// <returns>Total received balance of a given address.</returns>
        public ulong GetReceivedBalance(string address, int minConfirmations = 0)
        {
            return GetReceivedBalance(new[] { address }, minConfirmations);
        }

        /// <summary>
        /// Returns the sum of received (not current!) balance of given addresses.
        /// </summary>
        /// <param name="address">Cryptocurrency receiving addresses.</param>
        /// <param name="minConfirmations">Does not include the balance from transactions below this confirmation threshold. A value of zero allows the method to include all transactions, including unconfirmed ones.</param>
        /// <returns>Total received balance of a given address.</returns>
        public ulong GetReceivedBalance(IEnumerable<string> address, int minConfirmations = 0);
    }
}
