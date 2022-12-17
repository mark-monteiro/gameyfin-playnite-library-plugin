using System;
using System.Collections.Generic;
using System.Linq;

using Playnite.SDK.Data;
using Playnite.SDK;

namespace GameyfinLibrary.Views
{
    /// <summary>
    /// View model for <see cref="GameyfinLibrarySettingsView"/>.
    /// </summary>
    public class GameyfinLibrarySettingsViewModel : ObservableObject, ISettings
    {
        /// <summary>
        /// Gets or sets the plugin settings.
        /// </summary>
        public GameyfinLibrarySettings Settings
        {
            get => _settings;
            set => SetValue(ref _settings, value);
        }

        /// <summary>
        /// Comma separated list of platforms for <see cref="GameyfinLibrarySettings.ImportPlatforms"/>.
        /// </summary>
        public string PlatformFilter
        {
            get => string.Join(",", _settings.ImportPlatforms);
            set {
                var trimmedValue = value.Trim();
                _settings.ImportPlatforms = string.IsNullOrEmpty(trimmedValue) ?
                    Array.Empty<string>() :
                    trimmedValue.Split(',').Select(x => x.Trim()).ToArray();
                OnPropertyChanged(nameof(PlatformFilter));
            }
        }

        private readonly GameyfinLibrary _plugin;

        private GameyfinLibrarySettings _settings;
        private GameyfinLibrarySettings _editingClone;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameyfinLibrarySettingsViewModel"/> class. Used by the Visual
        /// Studio designer for binding intellisense.
        /// </summary>
        [Obsolete("Used at design-time only")]
        public GameyfinLibrarySettingsViewModel()
        {
            Settings = new GameyfinLibrarySettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameyfinLibrarySettingsViewModel"/> class.
        /// </summary>
        /// <param name="plugin">The Gameyfin library plugin.</param>
        public GameyfinLibrarySettingsViewModel(GameyfinLibrary plugin)
        {
            _plugin = plugin;
            Settings = _plugin.LoadPluginSettings<GameyfinLibrarySettings>() ?? new GameyfinLibrarySettings();
        }

        /// <summary>
        /// Executed when settings view is opened and user starts editing values.
        /// </summary>
        public void BeginEdit()
        {
            _editingClone = Serialization.GetClone(Settings);
        }

        /// <summary>
        /// Reverts any changes made to settings since <see cref="BeginEdit"/> was called.
        /// </summary>
        public void CancelEdit()
        {
            Settings = _editingClone;
        }

        /// <summary>
        /// Confirm changes made since <see cref="BeginEdit"/> was called.
        /// </summary>
        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
        }

        /// <inheritdoc/>
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            // Validated GameyfinUrl
            // TODO: implement by sending a HEAD or GET request to the gameyfin API and validating the response code
            var isValidGameyfinUrl = true;
            if (!isValidGameyfinUrl)
            {
                errors.Add($"{Settings.GameyfinUrl} is not a valid URL");
            }

            return !errors.Any();
        }
    }
}
