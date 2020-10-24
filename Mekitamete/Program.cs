using System;
using System.IO;

namespace Mekitamete
{
    class Program
    {
        static void Main(string[] args)
        {
            // if there's no settings file, use defaults and write them
            if (Settings.Instance == null)
            {

            }

            using (var mainApp = new MainApplication())
            {

            }
        }
    }
}
