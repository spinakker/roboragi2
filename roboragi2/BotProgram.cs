
using System;
using System.IO;

using Newtonsoft.Json;

using Discord;


namespace roboragi2
{
    class BotProgram
    {
        private BotConfig Configuration { get; set; }
        private DiscordClient client;

        public static void Main (string[] args) {
            if (!File.Exists ("config.json"))
                throw new FileNotFoundException ("The bot's config file (config.json) is missing.");

            var json = string.Empty;
            using (var fs = File.OpenRead ("config.json"))
            using (var sr = new StreamReader (fs))
                json = sr.ReadToEnd ();

            var cfg = JsonConvert.DeserializeObject<BotConfig> (json);

            new BotProgram (cfg).Start ();
        }

        public BotProgram (BotConfig config) {
            this.Configuration = config;
        }

        public void Start () {
            client = new DiscordClient();

            //stuff comes here

            client.Log.Message += (s, e) => Console.WriteLine ($"[{e.Severity}] {e.Source}: {e.Message}");

            client.ExecuteAndWait (async () => {
                await client.Connect (token: Configuration.Token, tokenType: TokenType.Bot);
            });
        }
    }
}