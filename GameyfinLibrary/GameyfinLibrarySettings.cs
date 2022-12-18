using System.Collections.Generic;

namespace GameyfinLibrary
{
    public class GameyfinLibrarySettings : ObservableObject
    {
        /// <summary>
        /// Gets or sets the URL of the Gameyfin server to import from.
        /// </summary>
        public string GameyfinUrl {
            get => _gameyfinUrl;
            set => SetValue(ref _gameyfinUrl, value);
        }

        /// <summary>
        /// Gets or sets the platforms filter to use when importing games from Gameyfin. Only games for the specified
        /// platforms will be imported. Playforms should be specified using their IGDB slug.
        /// </summary>
        public IReadOnlyCollection<string> ImportPlatforms
        {
            get => _importPlatforms;
            set => SetValue(ref _importPlatforms, value);
        }

        /// <summary>
        /// Gets or sets the method used to authenticate with Gameyfin.
        /// </summary>
        public GameyfinAuthMethod AuthMethod
        {
            get => _authMethod;
            set => SetValue(ref _authMethod, value);
        }

        /// <summary>
        /// Gets or sets the name of the auth cookie to pass along with requests to the Gameyfin API.
        /// </summary>
        public string AuthCookieName
        {
            get => _authCookieName;
            set => SetValue(ref _authCookieName, value);
        }

        /// <summary>
        /// Gets or sets the value of the auth cookie to pass along with requests to the Gameyfin API.
        /// </summary>
        public string AuthCookieValue
        {
            get => _authCookieValue;
            set => SetValue(ref _authCookieValue, value);
        }

        private string _gameyfinUrl = "";
        private IReadOnlyCollection<string> _importPlatforms = new string[] { "win" };
        private GameyfinAuthMethod _authMethod = GameyfinAuthMethod.None;
        private string _authCookieName = null;
        private string _authCookieValue = null;
    }

    /// <summary>
    /// An method that can be used to authenticate with the Gameyfin server.
    /// </summary>
    public enum GameyfinAuthMethod
    {
        /// <summary>
        /// No authentication.
        /// </summary>
        None,

        /// <summary>
        /// Forward-auth authentication (like Authelia)
        /// </summary>
        ForwardAuth
    }
}
