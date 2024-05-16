using NBABets.Models;
using System.Text.Json;

namespace NBABets.Client
{
    public class UserClient : BaseClient
    {
        private readonly HttpClient _httpClient;

        public UserClient(HttpClient httpClient) : base(httpClient)
        {
            string jsonPath = @$"{AppContext.BaseDirectory}\appsettings.json";
            string json;

            using (StreamReader reader = new StreamReader(jsonPath))
            {
                json = reader.ReadToEnd();
            }
            ConnectionDetails details = JsonSerializer.Deserialize<ConnectionDetails>(json);
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri($"https://{details.ApiURL}:{details.Port}/users/");
        }

        public async Task<UserDto> AuthoriseUser(string userName)
        {
            // Check to make sure the provided username isn't empty
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));
            string escapedUser = Uri.EscapeDataString(userName);
            string endpoint = @$"authorise?userName={escapedUser}";
            var response = await GetAsync<UserDto>(endpoint);
            return response;
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            string endpoint = "";
            var response = await GetListAsync<UserDto>(endpoint);
            if (response == null)
                return new List<UserDto>();
            return response;
        }

        public async Task<UserDto> GetUser(string IDorName)
        {
            string endpoint = $"{IDorName}";
            var response = await GetAsync<UserDto>(endpoint);
            return response;
        }
    }
}
