using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.API.Client;
using Discord.Commands;


namespace roboragi2
{
    internal class BotProgram
    {
        private BotConfig Configuration { get; set; }
        private DiscordClient _client;

        public static void Main (string[] args) {
            if (!File.Exists ("config.json"))
                throw new FileNotFoundException ("The bot's config file (config.json) is missing.");

            var json = string.Empty;
            using (var fs = File.OpenRead ("config.json"))
            using (var sr = new StreamReader (fs)) {
                json = sr.ReadToEnd ();
            }

            var cfg = JsonConvert.DeserializeObject<BotConfig> (json);

            new BotProgram (cfg).Start ();
        }

        public BotProgram (BotConfig config) {
            Configuration = config;
        }


        public void Start () {
            _client = new DiscordClient ();

            //stuff comes here

            // logging
            _client.Log.Message += (s, e) => Console.WriteLine ($"[{e.Severity}] {e.Source}: {e.Message}");

            // commands
            var commands = new Commands (ref _client);
            commands.CreateCommands ();

            _client.ExecuteAndWait (async () => { await _client.Connect (Configuration.Token, TokenType.Bot); });
        }
    }
}