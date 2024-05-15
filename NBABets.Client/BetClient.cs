using NBABets.Models;
using System.Text.Json;

namespace NBABets.Client
{
    public class BetClient : BaseClient
    {
        private readonly HttpClient _httpClient;
        public BetClient(HttpClient httpClient) : base(httpClient)
        {
            string jsonPath = @$"{AppContext.BaseDirectory}\appsettings.json";
            string json;

            using (StreamReader reader = new StreamReader(jsonPath))
            {
                json = reader.ReadToEnd();
            }
            ConnectionDetails details = JsonSerializer.Deserialize<ConnectionDetails>(json);
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri($"https://{details.ApiURL}:{details.Port}/bets/");
        }

        public async Task<List<BetDto>> GetAllBets()
        {
            string endpoint = "";
            var response = await GetListAsync<BetDto>(endpoint);
            if (response == null)
                return new List<BetDto>();
            return response;
        }

        public async Task<BetDto> GetBet(string IDorName)
        {
            string endpoint = $"{IDorName}";
            var response = await GetAsync<BetDto>(endpoint);
            return response;
        }

        public async Task<BetDto> AddBet(BetDto bet)
        {
            if (bet == null)
                throw new ArgumentNullException(nameof(bet));
            string endpoint = "add";
            var jsonRequest = JsonSerializer.Serialize(bet);
            var response = await PostAsync<BetDto>(endpoint, jsonRequest);
            return response;
        }

        public async Task<bool> EditBet(BetDto bet)
        {
            if (bet == null)
                throw new ArgumentNullException(nameof(bet));
            string endpoint = "edit";
            var jsonRequest = JsonSerializer.Serialize(bet);
            var response = await PutAsync<bool>(endpoint, jsonRequest);
            return response;
        }

        public async Task<bool> RemoveBet(string IDorName)
        {
            string endpoint = $"{IDorName}";
            var response = await DeleteAsync(endpoint);
            return response;
        }
    }
}
