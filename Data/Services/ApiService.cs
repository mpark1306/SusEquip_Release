using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers; // Specify the namespace for System.Timers.Timer
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;

namespace SusEquip.Data.Services
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;
        private System.Timers.Timer _timer; // Specify the full namespace
        private string _apiToken = string.Empty;

        public ApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;

            // Set up the timer to trigger every 2 minutes (120000 milliseconds)
            _timer = new System.Timers.Timer(120000); // Specify the full namespace
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // Initial token refresh
            RefreshApiToken();
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            RefreshApiToken();
        }

        public void RefreshApiToken()
        {
            var tokenResult = GetToken();
            _apiToken = tokenResult?.AccessToken ?? string.Empty; // Handle possible null value
        }

        public LoginTokenResult GetToken()
        {
            var username = _apiSettings.Username;
            var password = _apiSettings.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("API credentials are not set correctly.");
            }

            HttpClient client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://dtupcapi.ait.dtu.dk/token");

            HttpResponseMessage response = client.PostAsync("Token",
                new StringContent(string.Format("grant_type=password&username={0}&password={1}",
                    HttpUtility.UrlEncode(username),
                    HttpUtility.UrlEncode(password)), Encoding.UTF8,
                    "application/x-www-form-urlencoded")).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to retrieve token.");
            }

            string resultJSON = response.Content.ReadAsStringAsync().Result;
            LoginTokenResult result = JsonConvert.DeserializeObject<LoginTokenResult>(resultJSON) ?? new LoginTokenResult();

            return result;
        }

        // Async interface implementations
        public async Task<LoginTokenResult> GetTokenAsync()
        {
            return await Task.FromResult(GetToken());
        }

        public async Task RefreshApiTokenAsync()
        {
            await Task.Run(() => RefreshApiToken());
        }

        public async Task<bool> IsTokenValidAsync()
        {
            return await Task.FromResult(!string.IsNullOrEmpty(_apiToken));
        }
    }
}
