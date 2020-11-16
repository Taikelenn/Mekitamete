using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Mekitamete.Daemons;
using Mekitamete.Database;
using Mekitamete.Http;
using Mekitamete.Transactions;

namespace Mekitamete
{
    public class MainApplication : IDisposable
    {
        public static MainApplication Instance { get; private set; }
        public static MainApplication Create()
        {
            if (Instance != null)
            {
                return Instance;
            }

            Instance = new MainApplication();
            return Instance;
        }

        private bool shouldExit;
        internal DBConnection DBConnection { get; }
        private HttpInterface WebInterface { get; }
        private Dictionary<TransactionCurrency, ICryptoDaemon> CryptoDaemons { get; }

        private MainApplication()
        {
            DBConnection = new DBConnection();
            WebInterface = new HttpInterface(Settings.Instance.ServerPort);
            CryptoDaemons = new Dictionary<TransactionCurrency, ICryptoDaemon>();

            DBConnection.InitializeDatabase();

            CryptoDaemons.Add(TransactionCurrency.Monero, new MoneroDaemon(Settings.Instance.MoneroDaemon));
        }

        public ICryptoDaemon GetDaemonForCurrency(TransactionCurrency currency)
        {
            return CryptoDaemons.GetValueOrDefault(currency);
        }

        public void RequestTermination()
        {
            shouldExit = true;
        }

        public void Loop()
        {
            WebInterface.Listen();

            Thread.Sleep(Timeout.Infinite);

            WebInterface.Stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Instance != null)
            {
                if (disposing)
                {
                    DBConnection.Dispose();
                    Instance = null;
                }

                Instance = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
