using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Daemons.Requests
{
    class BitcoinGetTransactionResponse
    {
        public class BitcoinTransactionDetails
        {
            [JsonProperty("address")]
            public string Address { get; private set; }
            [JsonProperty("amount")]
            private decimal AmountDecimal { get; set; }
            public long Amount { get { return Decimal.ToInt64(AmountDecimal * 100000000M); } }
            [JsonProperty("vout")]
            public int OutputIndex { get; private set; }
            [JsonProperty("category")]
            public string Category { get; private set; }
        }

        [JsonProperty("confirmations")]
        public int Confirmations { get; private set; }
        [JsonProperty("details")]
        public BitcoinTransactionDetails[] Details { get; private set; }
        [JsonProperty("txid")]
        public string TxId { get; private set; }
    }
}
