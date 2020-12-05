using Newtonsoft.Json;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateAddressRequest
    {
        [JsonProperty("account_index")]
        public uint AccountIndex { get; }
        [JsonProperty("label")]
        public string Label { get; }

        public MoneroCreateAddressRequest(string label)
        {
            AccountIndex = 0; // use the default account
            Label = label;
        }
    }

    class MoneroCreateAddressResponse
    {
        [JsonProperty("address")]
        public string Address { get; private set; }
        [JsonProperty("address_index")]
        public uint AddressIndex { get; private set; }
    }
}
