/*
This file is part of Discord self bot.

Discord self bot is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Discord self bot is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Discord self bot.  If not, see<http://www.gnu.org/licenses/>.
*/


using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordSelfBot
{
    /// <summary>
    ///     This class contains the commands for the self bot
    /// </summary>
    public class Commands : ModuleBase
    {
        // ~say hello -> hello
        [Command("say")]
        [Summary("Echos a message.")]
        public async Task Say([Summary("The text to echo")] string echo)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(echo);
        }

        [Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfo([Summary("The guild member to get info for")] IGuildUser userInfo)
        {
                await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}: {userInfo.CreatedAt}\n" +
                                 $"Joined this guild at {userInfo.JoinedAt}");
        }
    }
}