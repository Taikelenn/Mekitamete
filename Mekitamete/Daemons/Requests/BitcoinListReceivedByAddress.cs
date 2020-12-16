using Newtonsoft.Json;

namespace Mekitamete.Daemons.Requests
{
    class BitcoinListReceivedByAddressResponse
    {
        [JsonProperty("address")]
        public string Address { get; private set; }
        [JsonProperty("txids")]
        public string[] TxIds { get; private set; }
    }
}
