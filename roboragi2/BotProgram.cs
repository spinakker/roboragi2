
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
                        await ArchivePics (e.Channel, Int32.Parse(e.GetArg ("numberOfPics")));
                    });
            });



            client.ExecuteAndWait (async () => {
                await client.Connect (token: Configuration.Token, tokenType: TokenType.Bot);
            });
        }

        private async Task ArchivePics (Channel channel, int archiveLimit = 0) {

            List<string> picUrLs = new List<string> ();

            await GetPicAddressesFromChannel (channel, picUrLs, null, archiveLimit);

            client.Log.Info ("number of pictures archived: ", picUrLs.Count.ToString());

            using (var webClient = new WebClient ()) {
                string newFolderName = channel.Name;
                if (!System.IO.Directory.Exists (newFolderName))
                  System.IO.Directory.CreateDirectory (newFolderName);

                foreach (var uriString in picUrLs) {
                    string filename = uriString.Split ('/').Last ();

                    if (!String.IsNullOrEmpty (filename) && filename.Length > 20) {
                        var extension = filename.Split ('.').Last ();
                        filename = filename.Substring (0, 10) + '.' + extension;
                    }

                    string path = newFolderName + '\\' + filename;

                    try {
                        webClient.DownloadFile (new Uri (uriString), path);
                    }
                    catch (Exception e) {
                        client.Log.Error (e.Source, e.Message);
                    }
                }
                
            }
        }

        private async Task GetPicAddressesFromChannel (Channel channel, List<string> picURLs, ulong? startFromThisMessageId, int numberOfPics) {

            const int maxLimit = 100;
            var messages = await channel.DownloadMessages (relativeMessageId: startFromThisMessageId, relativeDir: Relative.Before, limit: maxLimit);

            foreach (var message in messages) {
                var messageText = message.Text;
                string possiblePictureUrl = String.Empty;

                if (!messageText.StartsWith ("http"))
                    possiblePictureUrl = Regex.Match (messageText,
                        @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)").ToString();

                if (message.Attachments.Length > 0)
                    possiblePictureUrl = message.Attachments[0].Url;

                if (possiblePictureUrl.Length > 0) {
                    if (possiblePictureUrl.Length > 0 && picURLs.Count < numberOfPics)
                        picURLs.Add (possiblePictureUrl);
                    else
                        return;
                }
            }

            client.Log.Info ("number of messages found: ", messages.Length.ToString());

            if (messages.Length < maxLimit)
                return;
            else
                await GetPicAddressesFromChannel (channel, picURLs, messages.Last().Id, numberOfPics);

        }
    }
}