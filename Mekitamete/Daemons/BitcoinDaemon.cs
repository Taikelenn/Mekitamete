using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons
{
    class BitcoinDaemon : ICryptoDaemon
    {
        public string CreateNewAddress(string label)
        {
            throw new NotImplementedException();
        }

        public ulong GetReceivedBalance(IEnumerable<string> address, int minConfirmations = 0)
        {
            throw new NotImplementedException();
        }

        public BitcoinDaemon(RPCEndpointSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
