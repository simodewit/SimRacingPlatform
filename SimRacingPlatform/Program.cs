using System;
using Microsoft.UI.Xaml;
using Velopack;

namespace SimRacingPlatform
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            VelopackApp.Build().Run();

            Application.Start(_ => new App());
        }
    }
}
