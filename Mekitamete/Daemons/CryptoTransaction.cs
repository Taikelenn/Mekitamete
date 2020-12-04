using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Daemons
{
    public class CryptoTransaction
    {
        public string TxId { get; private set; }
        public string ReceivingAddress { get; private set; }
        public long ReceivingValue { get; private set; }
        public int Confirmations { get; private set; }

        public CryptoTransaction(string txId, string receivingAddr, long receivingValue, int confirmations)
        {
            TxId = txId;
            ReceivingAddress = receivingAddr;
            ReceivingValue = receivingValue;
            Confirmations = confirmations;
        }
    }
}
