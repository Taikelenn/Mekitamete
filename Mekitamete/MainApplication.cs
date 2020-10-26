﻿using System;
using Mekitamete.Database;
using Mekitamete.Http;

namespace Mekitamete
{
    public class MainApplication : IDisposable
    {
        private bool disposed;
        private DBConnection DBConnection { get; }
        private HttpInterface WebInterface { get; }

        public MainApplication()
        {
            DBConnection = new DBConnection();
            WebInterface = new HttpInterface(Settings.Instance.ServerPort);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DBConnection.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
