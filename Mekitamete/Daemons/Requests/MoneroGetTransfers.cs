using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mekitamete.Daemons.Requests
{
    class MoneroGetTransfersRequest
    {
        [JsonProperty("in")]
        public bool IncludeIncoming { get; }
        [JsonProperty("pool")]
        public bool IncludeInPool { get; }
        [JsonProperty("subaddr_indices")]
        public IEnumerable<uint> Addresses { get; }

        public MoneroGetTransfersRequest(IEnumerable<uint> addressIndices)
        {
            IncludeIncoming = true;
            IncludeInPool = true;
            Addresses = addressIndices ?? new List<uint>();
        }
    }

    class MoneroGetTransfersResponse
    {
        public class MoneroTransfer
        {
            [JsonProperty("address")]
            public string Address { get; private set; }
            [JsonProperty("amount")]
            public ulong Value { get; private set; }
            [JsonProperty("confirmations")]
            public uint Confirmations { get; private set; }
            [JsonProperty("height")]
            public uint BlockHeight { get; private set; }
            [JsonProperty("txid")]
            public string TxId { get; private set; }
        }

        [JsonProperty("in")]
        public IEnumerable<MoneroTransfer> IncomingTransfers { get; private set; }
        [JsonProperty("pool")]
        public IEnumerable<MoneroTransfer> PoolTransfers { get; private set; }
    }
}
