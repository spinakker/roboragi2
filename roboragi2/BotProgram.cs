
using System;
using System.IO;

using Newtonsoft.Json;

using Discord;
using Discord.Commands;


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

            // logging
            client.Log.Message += (s, e) => Console.WriteLine ($"[{e.Severity}] {e.Source}: {e.Message}");


            // commands
            client.UsingCommands (x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Public;
            });

            client.GetService<CommandService> ().CreateGroup ("do", cgb =>
            {
                cgb.CreateCommand ("greet")
                        .Alias (new string[] { "gr", "hi" })
                        .Description ("Greets a person.")
                        .Parameter ("GreetedPerson", ParameterType.Required)
                        .Do (async e =>
                        {
                            await e.Channel.SendMessage ($"{e.User.Name} greets {e.GetArg ("GreetedPerson")}");
                        });

                cgb.CreateCommand ("bye")
                        .Alias (new string[] { "bb", "gb" })
                        .Description ("Greets a person.")
                        .Parameter ("GreetedPerson", ParameterType.Required)
                        .Do (async e =>
                        {
                            await e.Channel.SendMessage ($"{e.User.Name} says goodbye to {e.GetArg ("GreetedPerson")}");
                        });
            });



            client.ExecuteAndWait (async () => {
                await client.Connect (token: Configuration.Token, tokenType: TokenType.Bot);
            });
        }
    }
}