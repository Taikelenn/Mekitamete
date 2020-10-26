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

            try
            {
                using (var mainApp = new MainApplication())
                {

                }
            }
            catch (Exception ex)
            {
                Logger.Log($"A top-level unhandled exception occurred and the program will now exit.\nDetails: {ex}", Logger.MessageLevel.Error);
            }
        }
    }
}
