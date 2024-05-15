using Microsoft.AspNetCore.Mvc;
using NBABets.Models;
using NBABets.Services;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using ILogger = Serilog.ILogger;

namespace NBABets.API.Controllers
{
    public class GamesController : ControllerBase
    {
        private readonly ILogger _log;
        private readonly IMapper<GameMapRequest, GameDto> _gameMapper;
        private readonly IGameAdapter _gameAdapter;
        private readonly IConfiguration _configuration;

        public GamesController(IGameAdapter gameAdapter, IMapper<GameMapRequest, GameDto> gameMapper, IConfiguration configuration)
        {
            _log = Log.ForContext<UserController>();
            _gameAdapter = gameAdapter;
            _gameMapper = gameMapper;
            _configuration = configuration;
        }

        [HttpGet("retrievegames", Name = "RetrieveGamesFromNBAApi")]
        public async Task<ActionResult> RetrieveGames()
        {
            // Craft call to NBA Api
            // season is always 1 year behind current year
            int season = DateTime.Now.Year - 1;

            // read NBAKey from appsettings.json
            string nbaKey = _configuration.GetValue<string>("NBAKey");

            // result of call
            List<NBAGame> games = new List<NBAGame>();

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://api-nba-v1.p.rapidapi.com/games?season={season}"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", nbaKey },
                        { "X-RapidAPI-Host", "api-nba-v1.p.rapidapi.com" },
                    },
                };

                // Get the Games
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    _log.Information("Getting current seasons games");
                    string jsonString = await response.Content.ReadAsStringAsync();

                    // Deserialise JSON into a jObject
                    dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(jsonString);

                    // get the response array from the JSON
                    var gamesJson = jsonResponse.response;

                    // deserialize each game in the array into an NBAGame
                    _log.Information("Deserialising games");
                    foreach (var gameJson in gamesJson)
                    {
                        NBAGame game = JsonConvert.DeserializeObject<NBAGame>(gameJson.ToString());
                        games.Add(game);
                        // get the logos
                        await DownloadLogos(game.teams.home.logo, game.teams.home.name + ".png");
                        await DownloadLogos(game.teams.visitors.logo, game.teams.visitors.name + ".png");
                    }
                }
            }

            // Craft the game objects
            foreach (var game in games)
            {
                Game newGame = new Game()
                {
                    ID = Guid.NewGuid(),
                    Name = $"{game.teams.home.name} VS {game.teams.visitors.name}",
                    StartDate = DateTime.ParseExact(game.date.start, "yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    Status = $"{game.status.@long}"
                };
                // sort date time
                if (game.date.end != null)
                {
                    newGame.EndDate = DateTime.ParseExact(game.date.end, "yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
                }

                // sort scores if null it's 0-0
                string homeScore = "0";
                string visitorScore = "0";

                if (game.scores.home.points != null)
                    homeScore = game.scores.home.points.ToString();
                if (game.scores.visitors.points != null)
                    visitorScore = game.scores.visitors.points.ToString();

                newGame.Score = $"{homeScore}-{visitorScore}";
                _gameAdapter.Add(newGame);
            }
            return Ok();
        }

        private async Task DownloadLogos(string logoUrl, string fileName)
        {
            // create path if it doesn't exist
            string path = @$"{Path.GetTempPath()}\NBALogos";
            string filePath = Path.Combine(path, fileName);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // only download if it doesn't exist
            if (!System.IO.File.Exists(filePath))
            {
                using (var client = new WebClient())
                {
                    // wikimedia requires a user-agent header
                    client.Headers.Add("User-Agent", "NBABetsAPI/1.0");
                    _log.Information($"Downloading logo: {logoUrl}");
                    await client.DownloadFileTaskAsync(logoUrl, filePath);

                }
            }
        }
    }
}
