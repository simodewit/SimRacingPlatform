using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using SimRacingPlatform.Utilities;
using System;
using Velopack;
using WinRT;

namespace SimRacingPlatform
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            VelopackApp.Build().Run();

            ComWrappersSupport.InitializeComWrappers();
            var activationArgs = SingleInstanceUtility.InitializeSingleInstance(OnInstanceActivated);

            if (activationArgs == null)
            {
                return;
            }

            SingleInstanceUtility.HandleProtocolActivation(activationArgs);

            Application.Start(_ => new App());
        }

        private static void OnInstanceActivated(object? sender, AppActivationArguments args)
        {
            // You can dispatch to the UI thread here if needed, or keep it simple.
            SingleInstanceUtility.HandleProtocolActivation(args);
        }
    }
}
