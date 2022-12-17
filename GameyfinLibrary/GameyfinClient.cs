using System.Diagnostics;

using GameyfinLibrary.Views;

using Playnite.SDK;

namespace GameyfinLibrary
{
    /// <summary>
    /// Gameyfin client that just opens the Gameyfin website in the default browser.
    /// </summary>
    public class GameyfinClient : LibraryClient
    {
        /// <inheritdoc/>
        public override bool IsInstalled => true;

        private readonly GameyfinLibrarySettingsViewModel _settingsVm;

        /// <summary>
        /// Constructs a new instance of the <see cref="GameyfinClient"/> class.
        /// </summary>
        /// <param name="settingsVm">The settings view model for the plugin.</param>
        public GameyfinClient(GameyfinLibrarySettingsViewModel settingsVm)
        {
            _settingsVm = settingsVm;
        }

        /// <inheritdoc/>
        public override void Open()
        {
            // Open the Gameyfin website in the default browser
            var gameyfinUrl = _settingsVm.Settings.GameyfinUrl;
            Process.Start(gameyfinUrl);
        }
    }
}
