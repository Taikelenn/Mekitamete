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

        private readonly HttpInterface WebInterface;
        private readonly Dictionary<TransactionCurrency, ICryptoDaemon> CryptoDaemons;

        private MainApplication()
        {
            DBConnection = new DBConnection();
            WebInterface = new HttpInterface(Settings.Instance.ServerPort);
            CryptoDaemons = new Dictionary<TransactionCurrency, ICryptoDaemon>();

            DBConnection.InitializeDatabase();

            // TODO: find a cleaner way to do it? preferably without resorting to reflection
            try
            {
                if (Settings.Instance.BitcoinDaemon == null)
                {
                    Logger.Log("Bitcoin", "Daemon disabled in configuration file", Logger.MessageLevel.Warning);
                }
                else
                {
                    CryptoDaemons.Add(TransactionCurrency.Bitcoin, new BitcoinDaemon(Settings.Instance.BitcoinDaemon));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Bitcoin", $"Daemon initialization failed: {ex.Message}", Logger.MessageLevel.Warning);
            }

            try
            {
                if (Settings.Instance.MoneroDaemon == null)
                {
                    Logger.Log("Monero", "Daemon disabled in configuration file", Logger.MessageLevel.Warning);
                }
                else
                {
                    CryptoDaemons.Add(TransactionCurrency.Monero, new MoneroDaemon(Settings.Instance.MoneroDaemon));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Monero", $"Daemon initialization failed: {ex.Message}", Logger.MessageLevel.Warning);
            }

            if (CryptoDaemons.Count == 0)
            {
                throw new InvalidOperationException("No cryptocurrency daemons were available.");
            }

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    shouldExit = true;
                    e.Cancel = true;
                }
            };
        }

        public ICryptoDaemon GetDaemonForCurrency(TransactionCurrency currency)
        {
            return CryptoDaemons.GetValueOrDefault(currency);
        }

        public void Loop()
        {
            WebInterface.Listen();

            while (true)
            {
                if (shouldExit)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

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
