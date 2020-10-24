using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete
{
    public class MainApplication : IDisposable
    {
        private bool disposed;

        public MainApplication()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
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
