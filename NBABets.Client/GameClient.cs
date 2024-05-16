using NBABets.Models;
using System.Text.Json;


namespace NBABets.Client
{
    public class GameClient : BaseClient
    {
        private readonly HttpClient _httpClient;
        public GameClient(HttpClient httpClient) : base(httpClient)
        {
            string jsonPath = @$"{AppContext.BaseDirectory}\appsettings.json";
            string json;

            using (StreamReader reader = new StreamReader(jsonPath))
            {
                json = reader.ReadToEnd();
            }
            ConnectionDetails details = JsonSerializer.Deserialize<ConnectionDetails>(json);
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri($"https://{details.ApiURL}:{details.Port}/games/");
        }

        public async Task<List<GameDto>> RetrieveGames()
        {
            string endpoint = "retrievegames";
            var response = await GetListAsync<GameDto>(endpoint);
            return response;
        }

        public async Task<List<GameDto>> GetAllGames()
        {
            string endpoint = "";
            var response = await GetListAsync<GameDto>(endpoint);
            if (response == null)
                return new List<GameDto>();
            return response;
        }

        public async Task<GameDto> GetGame(string IDorName)
        {
            string endpoint = $"{IDorName}";
            var response = await GetAsync<GameDto>(endpoint);
            return response;
        }
    }
}
