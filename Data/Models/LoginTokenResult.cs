using Newtonsoft.Json;

namespace SusEquip.Data.Models
{
    /// <summary>
    /// Result model for API login token response
    /// </summary>
    public class LoginTokenResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}