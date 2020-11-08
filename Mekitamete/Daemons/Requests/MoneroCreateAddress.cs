using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateAddressRequest
    {
        [JsonPropertyName("account_index")]
        public uint AccountIndex { get; set; }
        [JsonPropertyName("label")]
        public string Label { get; set; }

        public MoneroCreateAddressRequest(string label)
        {
            AccountIndex = 0; // use the default account
            Label = label;
        }
    }

    class MoneroCreateAddressResponse
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("address_index")]
        public uint AddressIndex { get; set; }
    }
}
