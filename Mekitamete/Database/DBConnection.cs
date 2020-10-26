using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Mekitamete.Database
{
    public partial class DBConnection : IDisposable
    {
        private const string DBFileName = "mekitamete.db";
        private SQLiteConnection dbConnection;

        private bool disposed;

        public DBConnection()
        {
            dbConnection = new SQLiteConnection($"Data Source={DBFileName}; Version=3");
            dbConnection.Open();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dbConnection.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
