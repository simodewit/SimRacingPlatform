using System;
using Microsoft.Windows.AppLifecycle;
using SimRacingPlatform.Pages;
using SimRacingPlatform.Windows;
using Windows.ApplicationModel.Activation;

namespace SimRacingPlatform.Utilities
{
    class SingleInstanceUtility
    {
        private const string InstanceKey = "main";

        public static AppActivationArguments? InitializeSingleInstance(
            EventHandler<AppActivationArguments> onActivated)
        {
            var current = AppInstance.GetCurrent();
            var activationArgs = current.GetActivatedEventArgs();

            var mainInstance = AppInstance.FindOrRegisterForKey(InstanceKey);

            if (!mainInstance.IsCurrent)
            {
                mainInstance.RedirectActivationToAsync(activationArgs).AsTask().Wait();
                return null;
            }

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

            var host = uri.Host?.ToLowerInvariant();

            Type? targetPage = null;

            switch (host)
            {
                case "verified":
                {
                    targetPage = typeof(EmailConfirmedPage);
                    break;
                }

                case "passwordresetdone":
                {
                    targetPage = typeof(PasswordChangedPage);
                    break;
                }

                default:
                {
                    break;
                }
            }

            if (targetPage != null)
            {
                MainWindow.Instance.NavigateTo(targetPage);
            }
        }
    }
}
