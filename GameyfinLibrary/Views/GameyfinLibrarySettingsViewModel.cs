using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Playnite.SDK.Data;
using Playnite.SDK;

using GameyfinLibrary.Services;

using AuthMethodOption = System.Collections.Generic.KeyValuePair<GameyfinLibrary.GameyfinAuthMethod, string>;

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
            private set => SetValue(ref _settings, value);
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

        /// <summary>
        /// Gets a collection of authentication options.
        /// </summary>
        public IReadOnlyCollection<AuthMethodOption> AuthMethodOptions { get; } = new AuthMethodOption[]
        {
            new AuthMethodOption(GameyfinAuthMethod.None, "No Authenticaton"),
            new AuthMethodOption(GameyfinAuthMethod.ForwardAuth, "Forward Authentication (Authelia, etc.)"),
        };

        /// <summary>
        /// Gets a command that can be used to initiate authentication.
        /// </summary>
        public ICommand AuthenticateCommand { get; }

        /// <inheritdoc cref="GameyfinWebviewAuthenticator.AuthenticationInProgress"/>
        public bool? AuthenticationInProgress => _gameyfinAuthenticator.AuthenticationInProgress;

        /// <inheritdoc cref="GameyfinWebviewAuthenticator.AuthenticateSuccess"/>
        public bool? AuthenticateSuccess => _gameyfinAuthenticator.AuthenticateSuccess;

        /// <inheritdoc cref="GameyfinWebviewAuthenticator.AuthenticationErrorMessage"/>
        public string AuthenticationErrorMessage => _gameyfinAuthenticator.AuthenticationErrorMessage;

        private readonly GameyfinLibrary _plugin;
        private readonly GameyfinWebviewAuthenticator _gameyfinAuthenticator;

        private GameyfinLibrarySettings _settings;
        private GameyfinLibrarySettings _editingClone;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameyfinLibrarySettingsViewModel"/> class. Used by the Visual
        /// Studio designer for binding intellisense.
        /// </summary>
        [Obsolete("Used at design-time only")]
        public GameyfinLibrarySettingsViewModel()
        {
            Settings = new GameyfinLibrarySettings
            {
                AuthMethod = GameyfinAuthMethod.ForwardAuth
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameyfinLibrarySettingsViewModel"/> class.
        /// </summary>
        /// <param name="plugin">The Gameyfin library plugin.</param>
        /// <param name="playniteApi">The Playnite API.</param>
        public GameyfinLibrarySettingsViewModel(GameyfinLibrary plugin, IPlayniteAPI playniteApi)
        {
            _plugin = plugin;
            _gameyfinAuthenticator = new GameyfinWebviewAuthenticator(this, playniteApi);
            Settings = _plugin.LoadPluginSettings<GameyfinLibrarySettings>() ?? new GameyfinLibrarySettings();
            AuthenticateCommand = new RelayCommand(Authenticate);

            // Respond to property changes in the authentication service
            _gameyfinAuthenticator.PropertyChanged += (s, e) =>
            {
                // Implement INotifyPropertyChanged for delegate properties
                if (e.PropertyName == nameof(_gameyfinAuthenticator.AuthenticationInProgress))
                    OnPropertyChanged(nameof(AuthenticationInProgress));
                if (e.PropertyName == nameof(_gameyfinAuthenticator.AuthenticateSuccess))
                    OnPropertyChanged(nameof(AuthenticateSuccess));
                if (e.PropertyName == nameof(_gameyfinAuthenticator.AuthenticationErrorMessage))
                    OnPropertyChanged(nameof(AuthenticationErrorMessage));
            };
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

        private void Authenticate()
        {
            _gameyfinAuthenticator.StartForwardAuthLogin();
        }
    }
}
