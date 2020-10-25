using System;
using System.IO;

namespace Mekitamete
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.OpenLogFile("mekitamete.log");
            Logger.Log("Initializing application...");

            using (var mainApp = new MainApplication())
            {

            }
        }
    }
}
