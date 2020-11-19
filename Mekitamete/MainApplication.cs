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

            // TODO: find a cleaner way to do it? preferably without resorting to reflection
            try
            {
                CryptoDaemons.Add(TransactionCurrency.Bitcoin, new BitcoinDaemon(Settings.Instance.BitcoinDaemon));
            }
            catch (Exception ex)
            {
                Logger.Log($"Bitcoin: daemon initialization failed: {ex.Message}", Logger.MessageLevel.Warning);
            }

            try
            {
                CryptoDaemons.Add(TransactionCurrency.Monero, new MoneroDaemon(Settings.Instance.MoneroDaemon));
            }
            catch (Exception ex)
            {
                Logger.Log($"Monero: daemon initialization failed: {ex.Message}", Logger.MessageLevel.Warning);
            }

            if (CryptoDaemons.Count == 0)
            {
                throw new InvalidOperationException("No cryptocurrency daemons were available.");
            }
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
