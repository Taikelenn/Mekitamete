﻿using System;
using System.Net;
using System.Threading;
using Mekitamete.Database;
using Mekitamete.Http;

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
        private DBConnection DBConnection { get; }
        private HttpInterface WebInterface { get; }

        private MainApplication()
        {
            DBConnection = new DBConnection();
            WebInterface = new HttpInterface(Settings.Instance.ServerPort);
        }

        public void RequestTermination()
        {
            shouldExit = true;
        }

        public void Loop()
        {
            WebInterface.Listen();

            Thread.Sleep(7000);

            WebInterface.Stop();
        }

        internal static byte[] HandleHTTPRequest(HttpListenerContext ctx)
        {
            ctx.Response.StatusCode = 404;
            return new byte[] { };
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
