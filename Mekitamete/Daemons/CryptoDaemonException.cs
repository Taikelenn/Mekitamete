using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons
{
    class CryptoDaemonException : Exception
    {
        public CryptoDaemonException()
        {
        }

        public CryptoDaemonException(string message) : base(message)
        {
        }

        public CryptoDaemonException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
