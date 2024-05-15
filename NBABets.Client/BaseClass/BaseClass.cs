using System.Text;
using System.Text.Json;


namespace NBABets.Client
{
    public abstract class BaseClient
    {
        JsonSerializerOptions _options;
        private readonly HttpClient _httpClient;
        public HttpClient HttpClient
        {
            get { return _httpClient; }
            set { }
        }

        public string Address
        {
            get { return _httpClient.BaseAddress?.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _httpClient.BaseAddress = new Uri(value);
                }
            }
        }
        public BaseClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            HttpResponseMessage response = await _httpClient.DeleteAsync(fullEndpoint);
            response.EnsureSuccessStatusCode();
            var result = bool.Parse(response.Content.ReadAsStringAsync().Result);
            return result;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(fullEndpoint);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<T>(jsonString, options);

            return result;
        }

        public async Task<string> GetStringAsync<T>(string endpoint)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(fullEndpoint);
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }

        public async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(fullEndpoint);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<List<T>>(jsonString, options);
            return result;
        }
        public async Task<byte[]> GetByteArrayAsync(string endpoint)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            HttpResponseMessage response = await _httpClient.GetAsync(fullEndpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<T> PostAsync<T>(string endpoint, string jsonContent)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(fullEndpoint, content);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(jsonString, _options);
            return result;
        }

        public async Task<T> PutAsync<T>(string endpoint, string content)
        {
            string fullEndpoint = _httpClient.BaseAddress + endpoint;
            var jsonContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(fullEndpoint, jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(jsonString, _options);
            return result;
        }

    }
}
