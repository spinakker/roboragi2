using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace roboragi2
{
    internal class Commands
    {
        public Commands (ref DiscordClient client) {
            _client = client;
        }

        private bool CheckIfUserHasRole (Channel channel, String roleName, User user) {
            var roles = channel.Server.FindRoles (roleName);
            foreach (var role in roles) {
                if (role != null)
                    return user.HasRole (role);
            }
            return false;
        }

        private DiscordClient _client;

        public void CreateCommands () {
            _client.UsingCommands (x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Public;
            });

            _client.GetService<CommandService> ().CreateGroup ("do", cgb => {
                cgb.CreateCommand ("greet")
                    .Alias (new string[] {"gr", "hi"})
                    .Description ("Greets a person.")
                    .Parameter ("GreetedPerson", ParameterType.Required)
                    .Do (
                        async e => {
                            await e.Channel.SendMessage ($"{e.User.Name} greets {e.GetArg ("GreetedPerson")}");
                        });

                cgb.CreateCommand ("bye")
                    .Alias (new string[] {"bb", "gb"})
                    .Description ("Greets a person.")
                    .Parameter ("GreetedPerson", ParameterType.Required)
                    .Do (
                        async e => {
                            await e.Channel.SendMessage ($"{e.User.Name} says goodbye to {e.GetArg ("GreetedPerson")}");
                        });
            });

            _client.GetService<CommandService> ().CreateGroup ("archive", cgb => {
                cgb.CreateCommand ("pics")
                    .Description ("Saves pictures from channel.")
                    .Parameter ("numberOfPics", ParameterType.Optional)
                    .AddCheck (
                        (command, user, channel) => CheckIfUserHasRole (channel, "King of the Weebs", user))
                    .Do (async e => {
                        var archiver = new Archiver (ref _client);
                        await archiver.ArchivePics (e.Channel, int.Parse (e.GetArg ("numberOfPics")));
                    });
            });
        }
    }
}