using Newtonsoft.Json;

namespace roboragi2 {
    public sealed class BotConfig {
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
