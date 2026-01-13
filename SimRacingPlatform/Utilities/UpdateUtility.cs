using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SimRacingPlatform.Utilities
{
    public enum UpdateResult
    {
        NotInstalled,
        UpToDate,
        Restarting
    }

    public sealed class UpdateUtility
    {
        public static async Task<UpdateResult> RunUpdateFlowAsync(Action<int>? onProgress = null, CancellationToken cancellationToken = default)
        {
            UpdateManager updateManager = CreateUpdateManager();

            // If the app is not running from a Velopack install (e.g. F5 from Visual Studio)
            if (!updateManager.IsInstalled)
            {
                return UpdateResult.NotInstalled;
            }

            // If an update was already downloaded and is waiting for a restart
            if (updateManager.UpdatePendingRestart != null)
            {
                updateManager.ApplyUpdatesAndRestart(updateManager.UpdatePendingRestart);
                return UpdateResult.Restarting;
            }

            UpdateInfo? updateInfo = await updateManager.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                return UpdateResult.UpToDate;
            }

            // Download the update (progress is reported as 0–100)
            await updateManager.DownloadUpdatesAsync(
                updateInfo,
                progress =>
                {
                    if (onProgress != null)
                    {
                        onProgress(progress);
                    }
                },
                cancellationToken
            );

            updateManager.ApplyUpdatesAndRestart(updateInfo);
            return UpdateResult.Restarting;
        }

        private static UpdateManager CreateUpdateManager()
        {
            GithubSource source = new GithubSource(
                repoUrl: "https://github.com/YourOrg/YourRepo",
                accessToken: "",
                prerelease: false
            );

            return new UpdateManager(source);
        }
    }
}
