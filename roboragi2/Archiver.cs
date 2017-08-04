using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;


namespace roboragi2 {
    internal class Archiver {
        public Archiver(ref DiscordClient clientPar) {
            _client = clientPar;
        }

        private readonly DiscordClient _client;

        public async Task ArchivePics(Channel channel, int archiveLimit = 0) {
            List<string> picUrLs = new List<string>();

            await GetPicAddressesFromChannel(channel, picUrLs, null, archiveLimit);

            _client.Log.Info(this.ToString(), "Saving " + picUrLs.Count.ToString() + " pictures...");

            using (var webClient = new WebClient()) {
                string newFolderName = channel.Name;
                if (!System.IO.Directory.Exists(newFolderName))
                    System.IO.Directory.CreateDirectory(newFolderName);

                foreach (var uriString in picUrLs) {
                    string filename = uriString.Split('/').Last();

                    if (!string.IsNullOrEmpty(filename) && filename.Length > 20) {
                        var extension = filename.Split('.').Last();
                        filename = filename.Substring(0, 10) + '.' + extension;
                    }

                    string path = newFolderName + '\\' + filename;

                    try {
                        webClient.DownloadFile(new Uri(uriString), path);
                    }
                    catch (Exception e) {
                        _client.Log.Error(e.Source, e.Message);
                    }
                    _client.Log.Info(this.ToString(), filename + " has been saved to: " + path);
                }
            }
        }

        private async Task GetPicAddressesFromChannel(Channel channel, ICollection<string> picUrLs,
            ulong? startFromThisMessageId, int numberOfPics) {
            const int maxLimit = 100;
            var messages = await channel.DownloadMessages(relativeMessageId: startFromThisMessageId,
                relativeDir: Relative.Before, limit: maxLimit);

            foreach (var message in messages) {
                var messageText = message.Text;
                var possiblePictureUrl = string.Empty;

                if (!messageText.StartsWith("http"))
                    possiblePictureUrl = Regex.Match(messageText,
                            @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)")
                        .ToString();

                if (message.Attachments.Length > 0)
                    possiblePictureUrl = message.Attachments[0].Url;

                if (possiblePictureUrl.Length <= 0)
                    continue;
                if (possiblePictureUrl.Length > 0 && picUrLs.Count < numberOfPics)
                    picUrLs.Add(possiblePictureUrl);
                else
                    return;
            }

            _client.Log.Info("Number of pictures found: ", messages.Length.ToString());

            if (messages.Length < maxLimit)
                return;
            await GetPicAddressesFromChannel(channel, picUrLs, messages.Last().Id, numberOfPics);
        }

        public async Task ArchiveUser(ulong userID, Channel channel) {
            const int maxLimit = 100;

            ulong? startFromThisMessageId = null;
            var tempMessages = await channel.DownloadMessages(relativeMessageId: startFromThisMessageId,
                relativeDir: Relative.Before, limit: maxLimit);
            var userMessages = new List<Message>();

            do {
                foreach (var message in tempMessages) {
                    if (message.User != null) {
                        if (message.User.Id == userID) {
                            userMessages.Add(message);
                            _client.Log.Info(this.ToString(), "found a message:" + message.ToString());
                        }
                    }
                    else {
                        var messageString = message.ToString();
                        // todo make it work (username as param for command)
                    }
                }

                tempMessages = await channel.DownloadMessages(relativeMessageId: tempMessages.Last().Id,
                    relativeDir: Relative.Before, limit: maxLimit);
            } while (tempMessages.Length == maxLimit);

            TextWriter tw = new StreamWriter(userID.ToString() + ".txt");

            foreach (var userMessage in userMessages) {
                tw.WriteLine(userMessage);
            }

            tw.Close();
        }
    }
}