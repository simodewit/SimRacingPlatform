using System;
using Microsoft.Windows.AppLifecycle;
using SimRacingPlatform.Pages;
using SimRacingPlatform.Windows;
using Windows.ApplicationModel.Activation;

namespace SimRacingPlatform.Utilities
{
    class SingleInstanceUtility
    {
        private const string InstanceKey = "main"; // any fixed key

        public static AppActivationArguments? InitializeSingleInstance(
            EventHandler<AppActivationArguments> onActivated)
        {
            var current = AppInstance.GetCurrent();
            var activationArgs = current.GetActivatedEventArgs();

            // Register or find the main instance
            var mainInstance = AppInstance.FindOrRegisterForKey(InstanceKey);

            if (!mainInstance.IsCurrent)
            {
                // This is a second instance: redirect its activation to the main one and exit
                mainInstance.RedirectActivationToAsync(activationArgs).AsTask().Wait();
                return null;
            }

            // We ARE the main instance -> listen for future activations (e.g. more protocol calls)
            current.Activated += onActivated;

            return activationArgs;
        }

        public static void HandleProtocolActivation(AppActivationArguments args)
        {
            if (args == null)
            {
                return;
            }

            if (args.Kind != ExtendedActivationKind.Protocol)
            {
                return;
            }

            if (args.Data is not IProtocolActivatedEventArgs protocolArgs)
            {
                return;
            }

            Uri uri = protocolArgs.Uri;

            MainWindow.Instance.NavigateTo(typeof(EmailConfirmedPage));
        }
    }
}
