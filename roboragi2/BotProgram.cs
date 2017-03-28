using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Commands;

using Newtonsoft.Json;


namespace roboragi2
{
    class BotProgram
    {
        private BotConfig Configuration { get; set; }
        private DiscordClient Client { get; set; }
        private CommandModule Commands { get; set; }

        public static void Main (string[] args) {
            if (!File.Exists ("config.json"))
                throw new FileNotFoundException ("The bot's config file (config.json) is missing.");

            var json = string.Empty;
            using (var fs = File.OpenRead ("config.json"))
            using (var sr = new StreamReader (fs))
                json = sr.ReadToEnd ();

            var cfg = JsonConvert.DeserializeObject<BotConfig> (json);

            var bot = new BotProgram (cfg);

            bot.Run ().GetAwaiter ().GetResult ();
        }

        public BotProgram (BotConfig config) {
            this.Configuration = config;
        }


        public  async Task Run () {

            Client = new DiscordClient (new DiscordConfig {
                Token = this.Configuration.Token,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Debug,
                AutoReconnect = true
            });

            this.Client.DebugLogger.LogMessageReceived += (o, e) =>
            {
                Console.WriteLine ($"[{e.TimeStamp.ToString ("yyyy-MM-dd HH:mm:ss")}] [{e.Level}] {e.Message}");
            };

            this.Commands = this.Client.UseCommands (new CommandConfig {
                Prefix = "!",
                SelfBot = false
            });

            this.Commands.AddCommand ("hello", async e => {
                await e.Message.Respond ($"Hello, {e.Message.Author.Mention}!");
            });

            this.Commands.AddCommand ("ping", async e => {
                await e.Message.Respond ("pong");
            });

            await this.Client.Connect ();

            await Task.Delay (-1);
        }

        private void DebugLogger_LogMessageReceived (object sender, DebugLogMessageEventArgs e) {
            Console.WriteLine ($"[{e.TimeStamp.ToString ("yyyy-MM-dd HH:mm:ss")}] [{e.Level}] {e.Message}");
        }
    }
}