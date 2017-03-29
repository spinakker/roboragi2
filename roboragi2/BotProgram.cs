
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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

            client.GetService<CommandService> ().CreateGroup ("archive", cgb => {
                cgb.CreateCommand ("pics")
                    .Description ("Saves pictures from channel.")
                    .Parameter ("numberOfPics", ParameterType.Optional)
                    .Do (async e => {
                        await ArchivePics (e.Channel);
                    });
            });



            client.ExecuteAndWait (async () => {
                await client.Connect (token: Configuration.Token, tokenType: TokenType.Bot);
            });
        }

        private async Task ArchivePics (Channel channel, int archiveLimit = 0) {

            List<string> picURLs = new List<string> ();

            await GetPicAddressesFromChannel (channel, picURLs);

            foreach (var picURL in picURLs) {
                client.Log.Info ("picture was found in channel:", picURL);
            }

            using (var webClient = new WebClient ()) {
                webClient.DownloadFileAsync (new Uri (picURLs [0]), "testpic.jpeg");
            }
        }

        private async Task GetPicAddressesFromChannel (Channel channel, List<string> picURLs, ulong? startFromThisMessageId = null) {

            const int maxLimit = 100;
            var messages = await channel.DownloadMessages (relativeMessageId: startFromThisMessageId, relativeDir: Relative.Before, limit: maxLimit);

            foreach (var message in messages) {
                if (message.Text.StartsWith ("http"))
                    picURLs.Add (message.Text);
            }

            client.Log.Info ("number of messages found: ", messages.Length.ToString());

            if (messages.Length < maxLimit)
                return;
            else
                await GetPicAddressesFromChannel (channel, picURLs, messages.Last().Id);

        }
    }
}