using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

using GameyfinLibrary.Views;
using GameyfinLibrary.Services;

namespace GameyfinLibrary
{
    public class GameyfinLibrary : LibraryPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(GameyfinLibrary));

        /// <inheritdoc/>
        public override Guid Id { get; } = Guid.Parse("8a73284a-7f62-4a09-9ac8-e25fcbdaabb3");

        /// <inheritdoc/>
        public override string Name { get; } = "Gameyfin";

        public override string LibraryIcon { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\gameyfin-icon.png");

        /// <inheritdoc/>
        public override LibraryClient Client { get; }

        private readonly GameyfinLibrarySettingsViewModel _settingsVm;
        private readonly IPlayniteAPI _playniteApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameyfinLibrary"/> class.
        /// </summary>
        /// <param name="playniteApi">The Playnite API.</param>
        public GameyfinLibrary(IPlayniteAPI playniteApi)
            : base(playniteApi)
        {
            _settingsVm = new GameyfinLibrarySettingsViewModel(this, playniteApi);
            _playniteApi = playniteApi;

            Client = new GameyfinClient(_settingsVm);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        /// <inheritdoc/>
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            using (var gameInfoFetcher = new GameyfinGameFetcher(_settingsVm.Settings, _playniteApi))
            {
                return gameInfoFetcher.GetGames(args).Result;
            }
        }

        /// <inheritdoc/>
        public override LibraryMetadataProvider GetMetadataDownloader() => 
            base.GetMetadataDownloader();

        /// <inheritdoc/>
        public override ISettings GetSettings(bool firstRunSettings) =>
            _settingsVm;

        /// <inheritdoc/>
        public override UserControl GetSettingsView(bool firstRunSettings) =>
            new GameyfinLibrarySettingsView();
    }
}
