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

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordSelfBot
{
    /// <summary>
    ///     This class is used to create a self bot.
    /// </summary>
    internal class SelfBot : IDisposable
    {
        private readonly LoginForm _callback;
        private readonly CommandService _commands;
        private readonly DependencyMap _map;
        private readonly string _token;
        private DiscordSocketClient _client;

        /// <summary>
        ///     Constructor of this class
        /// </summary>
        /// <param name="token">userToken</param>
        /// <param name="callback">The loginForm that created this object</param>
        public SelfBot(string token, LoginForm callback)
        {
            _callback = callback;
            _map = new DependencyMap();
            _commands = new CommandService();
            _token = token;
        }

        /// <summary>
        ///     Disposes of the discord client
        /// </summary>
        public async void Dispose()
        {
            await _client.DisconnectAsync();
            _client.Dispose();
            _client = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Start connecting with the discord server in a new thread
        /// </summary>
        /// <returns>DiscordSocketClient object that was used.</returns>
        public DiscordSocketClient Start()
        {
            _client = new DiscordSocketClient();
            new Thread(Run) {IsBackground = true}.Start();
            return _client;
        }

        /// <summary>
        ///     Runs the bot, Exceptions are send to the callback.ExceptionNotifier
        /// </summary>
        /// <see cref="LoginForm" />
        public async void Run()
        {
            try
            {
                // Configure the client to use a Bot token, and use our token
                await _client.LoginAsync(TokenType.User, _token);

                // Connect the client to Discord's gateway
                await _client.ConnectAsync();

                //activate commands
                await InstallCommands();
            }
            catch (Exception e)
            {
                _callback.ExceptionNotifier(e);
            }
        }

        /// <summary>
        ///     Instals commands
        /// </summary>
        /// <returns>Task</returns>
        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /// <summary>
        ///     This method handles the commands that are recieved
        /// </summary>
        /// <param name="messageParam">The message that will be checked for commands</param>
        /// <returns>Task</returns>
        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if ((message == null) || (message.Author.Id != message.Discord.CurrentUser.Id)) return;
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('&', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            // Create a Command Context
            var context = new CommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await _commands.ExecuteAsync(context, argPos, _map);
            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}