using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.Json;
using NBABets.Models;
using NBABets.Services;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using ILogger = Serilog.ILogger;

namespace NBABets.API.Controllers
{
    [ApiController]
    [Route("games")]
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

        /// <summary>
        /// Gets all games in the database
        /// </summary>
        /// <returns>A list of Games</returns>
        [HttpGet("", Name = "GetAllGames")]
        public ActionResult<List<GameDto>> GetAllGames()
        {
            List<Game> games = _gameAdapter.GetAll();

            // map games
            List<GameDto> gameDtos = games.Select(game =>
            {
                GameMapRequest request = new GameMapRequest { game = game };
                return _gameMapper.Map(request);
            }).ToList();

            return gameDtos;
        }

        /// <summary>
        /// Retrieves all the games from the current NBA season but only saves ones that are in the future or currently being played
        /// </summary>
        /// <returns></returns>
        [HttpGet("retrievegames", Name = "RetrieveGamesFromNBAApi")]
        public async Task<ActionResult<List<GameDto>>> RetrieveGames()
        {
            // Read the latest stored date
            string secretsPath = $@"{AppContext.BaseDirectory}\secrets.json";

            string jsonContent = System.IO.File.ReadAllText(secretsPath);

            Secrets secrets = JsonConvert.DeserializeObject<Secrets>(jsonContent);
            var getGames = _gameAdapter.GetAll();

            if (DateTime.Today < secrets.LastStoredDate)
            {
                _log.Information("Games already up-to-date");
                List<GameDto> returnGames = getGames.Select(game =>
                {
                    GameMapRequest request = new GameMapRequest { game = game };
                    return _gameMapper.Map(request);
                }).ToList();

                return returnGames;
            }
            // Get new games, clear out the ones currently in DB - won't try if it's the first time and the db is empty
            if (getGames.Any())
            {
                foreach (var game in getGames)
                {
                    DeleteGames(game.ID);
                }
            }

            // Craft call to NBA Api
            // season is always 1 year behind current year
            int season = DateTime.Now.Year - 1;

            // read NBAKey from appsettings.json
            string nbaKey = secrets.NBAKey;

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
                        string path = Path.Combine(@$"{Path.GetTempPath()}\NBALogos");

                        if (Directory.GetFiles(path).Length != 29)
                        {
                            await DownloadLogos(game.teams.home.logo, game.teams.home.name + ".png");
                            await DownloadLogos(game.teams.visitors.logo, game.teams.visitors.name + ".png");
                        }

                    }
                }
            }

            // Craft the game objects
            foreach (var game in games)
            {
                if (game.status.@long != "Finished")
                {
                    Game newGame = new Game()
                    {
                        ID = Guid.NewGuid(),
                        Name = $"{game.teams.home.name} VS {game.teams.visitors.name}",
                        StartDate = DateTime.ParseExact(game.date.start, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture),
                        Status = $"{game.status.@long}"
                    };
                    // sort date time
                    if (game.date.end != null)
                    {
                        newGame.EndDate = DateTime.ParseExact(game.date.end, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
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
            }
            UpdateSecretsJson();
            var createdGames = _gameAdapter.GetAll();

            List<GameDto> result = createdGames.Select(game =>
            {
                GameMapRequest request = new GameMapRequest { game = game };
                return _gameMapper.Map(request);
            }).ToList();

            return result;
        }

        /// <summary>
        /// Gets a game by it's ID or name
        /// </summary>
        /// <param name="IDorName"></param>
        /// <returns>A single game dto</returns>
        [HttpGet("{IDorName}", Name = "GetGame")]
        public ActionResult<GameDto> GetGame([FromRoute] string IDorName)
        {
            Game game = _gameAdapter.Get(IDorName);

            if (game != null)
            {
                GameMapRequest gameMapRequest = new GameMapRequest() 
                { 
                    game = game,
                };
                return _gameMapper.Map(gameMapRequest);
            }
            _log.Information($"Game not found: {IDorName}");
            return BadRequest($"Game not found: {IDorName}");
        }

        /// <summary>
        /// Deletes games from the DB - shouldn't be API facing
        /// </summary>
        /// <param name="gameID"></param>
        private void DeleteGames(Guid gameID)
        {
            _log.Information($"Deleting game: {gameID.ToString()}");
            _gameAdapter.Delete(gameID.ToString());
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

        private void UpdateSecretsJson()
        {
            string jsonFilePath = $@"{AppContext.BaseDirectory}\secrets.json";

            // read the JSON file
            string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

            // turn into a secrets object
            Secrets secrets = JsonConvert.DeserializeObject<Secrets>(jsonContent);

            // update properties
            secrets.LastStoredDate = DateTime.Today.AddDays(7);

            // turn back into JSON and save it
            using (StreamWriter file = System.IO.File.CreateText(jsonFilePath))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(file, secrets);

            }
        }
    }
}
