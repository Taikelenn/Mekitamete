using Mekitamete.Daemons;
using Mekitamete.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Http.Responses
{
    public class HttpStatusCheckResponse : HttpJsonResponse
    {
        public class DaemonStatus
        {
            public long BytesSent { get; private set; }
            public long BytesReceived { get; private set; }
            public int InConnections { get; private set; }
            public int MempoolTransactions { get; private set; }
            public int BlockHeight { get; private set; }

            public DaemonStatus(long bytesSent, long bytesReceived, int connections, int mempoolTransactions, int blockHeight)
            {
                BytesSent = bytesSent;
                BytesReceived = bytesReceived;
                InConnections = connections;
                MempoolTransactions = mempoolTransactions;
                BlockHeight = blockHeight;
            }
        }

        public DaemonStatus Bitcoin { get; private set; }
        public DaemonStatus Monero { get; private set; }

        public HttpStatusCheckResponse()
        {
            throw new NotImplementedException();
            //ICryptoDaemon bitcoinDaemon = MainApplication.Instance.GetDaemonForCurrency(TransactionCurrency.Bitcoin);
            //if (bitcoinDaemon != null)
            //{
            //
            //}
        }
    }
}
