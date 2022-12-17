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

        private string _gameyfinUrl = "";
        private IReadOnlyCollection<string> _importPlatforms = new string[] { "win" };
    }
}
