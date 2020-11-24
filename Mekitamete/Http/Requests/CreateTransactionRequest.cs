using Mekitamete.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Requests
{
    public class CreateTransactionRequest
    {
        [JsonProperty(Required = Required.Always)]
        public TransactionCurrency Currency { get; private set; }
        [JsonProperty(Required = Required.Always)]
        public ulong Value { get; private set; }
        public string Notes { get; } = null;
    }
}
