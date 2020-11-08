using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons
{
    interface ICryptoDaemon
    {
        /// <summary>
        /// Creates a new address for receiving cryptocurrency. The address may be assigned an optional label.
        /// </summary>
        /// <param name="label">The address label; can be null.</param>
        /// <returns>A newly created address.</returns>
        /// <exception cref="CryptoDaemonException"></exception>
        public string CreateNewAddress(string label);
    }
}
