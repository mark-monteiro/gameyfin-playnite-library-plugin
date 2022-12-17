using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

using GameyfinLibrary.Models;

namespace GameyfinLibrary.Services
{
    /// <summary>
    /// Service class for fetching game metadata from Gameyfin.
    /// </summary>
    internal sealed class GameyfinGameFetcher : IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(GameyfinGameFetcher));

        private readonly GameyfinLibrarySettings _settings;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructs a new instance of the <see cref="GameyfinGameFetcher"/> class.
        /// </summary>
        /// <param name="settingsVm">The plugin settings.</param>
        public GameyfinGameFetcher(GameyfinLibrarySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // Set an authentication cookie
            // TODO: Implement forward-auth correctly
            var baseAddress = new Uri(_settings.GameyfinUrl);
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(baseAddress, new Cookie("authelia_session", "rhmAMrQD_ExEZOEFiabr%zNO29M6p7bv"));

            // Create an HTTP client with the Gameyfin server address as the base address
            _httpClientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
            _httpClient = new HttpClient(_httpClientHandler) { BaseAddress = baseAddress };

        }

        /// <inheritdoc cref="LibraryPlugin.GetGames(LibraryGetGamesArgs)"/>
        public async Task<IEnumerable<GameMetadata>> GetGames(LibraryGetGamesArgs args)
        {
            // Send request to fetch games
            var result = await _httpClient.GetAsync("/v1/games", args.CancelToken);

            // Validate response code
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Request to fetch games from Gameyfin failed with response code {result.StatusCode} ({result.ReasonPhrase})");

            // Parse the response body JSON
            var responseBody = await result.Content.ReadAsStringAsync();
            var games = Serialization.FromJson<GameyfinGame[]>(responseBody).AsEnumerable();

            // Filter our games that have not been confirmed as a match in Gameyfin
            games = games.Where(game => game.ConfirmedMatch);

            // Optionally filter out games that do not match the platform filter
            if (_settings.ImportPlatforms.Any())
                games = games.Where(game => game.Library.Platforms.Select(x => x.Slug).Intersect(_settings.ImportPlatforms).Any());

            // Convert each returned game into a GameMetadata object
            return games.Select(GetMetadataForGame);  
        }

        private GameMetadata GetMetadataForGame(GameyfinGame game)
        {
            return new GameMetadata
            {
                Source = new MetadataNameProperty("Gameyfin"),

                Name = game.Title,
                GameId = game.Slug,
                Description = game.Summary,
                ReleaseDate = new ReleaseDate(DateTime.Parse(game.ReleaseDate)),

                UserScore = game.UserRating,
                CriticScore = game.CriticsRating,

                Genres = game.Genres.Select(x => new MetadataNameProperty(x.Name)).Cast<MetadataProperty>().ToHashSet(),
                Platforms = game.Library.Platforms.Select(x => new MetadataNameProperty(x.Name)).Cast<MetadataProperty>().ToHashSet(),

                Features = GetFeatures(game).Select(x => new MetadataNameProperty(x)).Cast<MetadataProperty>().ToHashSet(),

                Developers = game.Companies.Select(x => new MetadataNameProperty(x.Name)).Cast<MetadataProperty>().ToHashSet(),
                // Publishers =

                CoverImage = GetImageMetadataFile(game.CoverId),
                //BackgroundImage = 
                //Icon = 

                IsInstalled = false,
                GameActions = new List<GameAction> {
                    new GameAction
                    {
                        Name = "Download",
                        IsPlayAction = false,
                        Type = GameActionType.URL,
                        Path = new Uri(_httpClient.BaseAddress, $"/v1/games/game/{game.Slug}/download").ToString(),
                    }
                }
            };
        }

        private IEnumerable<string> GetFeatures(GameyfinGame game)
        {
            if (game.OnlineCoop)
                yield return "Online Co-Op";
            if (game.OfflineCoop)
                yield return "Offline Co-Op";
            if (game.LanSupport)
                yield return "LAN Support";

            foreach (var perspective in game.PlayerPerspectives)
            {
                yield return perspective.Name;
            }
        }

        private MetadataFile GetImageMetadataFile(string gameyfinImageId)
        {

            var imageUri = new Uri(_httpClient.BaseAddress, "/v1/images/" + gameyfinImageId);
            return new MetadataFile(imageUri.ToString());
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _httpClientHandler.Dispose();
        }
    }
}
