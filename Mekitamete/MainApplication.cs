using System;
using Mekitamete.Database;

namespace Mekitamete
{
    public class MainApplication : IDisposable
    {
        private bool disposed;
        private DBConnection DBConnection { get; }

        public MainApplication()
        {
            DBConnection = new DBConnection();
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
