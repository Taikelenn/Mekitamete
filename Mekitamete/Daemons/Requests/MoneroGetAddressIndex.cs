using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Daemons.Requests
{
    class MoneroGetAddressIndexRequest
    {
        [JsonProperty("address")]
        public string Address { get; }

        public MoneroGetAddressIndexRequest(string address)
        {
            Address = address;
        }
    }

    class MoneroGetAddressIndexResponse
    {
        private class MoneroAddrIndex
        {
            [JsonProperty("major")]
            internal uint Major { get; private set; }
            [JsonProperty("minor")]
            internal uint Minor { get; private set; }
        }

        [JsonProperty("index")]
        private MoneroAddrIndex Index { get; set; }

        public uint IndexMajor { get { return Index.Major; } }
        public uint IndexMinor { get { return Index.Minor; } }
    }
}
