﻿using System;
using System.IO;

namespace Mekitamete
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.OpenLogFile("mekitamete.log");
            Logger.Log("Main", "Initializing application...");

            try
            {
                using (var mainApp = MainApplication.Create())
                {
                    mainApp.Loop();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Main", $"A top-level unhandled exception occurred and the program will now exit.\nDetails: {ex}", Logger.MessageLevel.Error);
            }
        }
    }
}
