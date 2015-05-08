using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IrcClient
{
    public partial class Client
    {

        /// <summary>
        /// Handler for a user executing a "/msg" command. Sends the message directly
        /// to another user rather than to the whole channel.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task PrivateMessageCommandSent(UserCommand command)
        {
            if (command.Parameters.Count < 2) return;

            var receivingUser = command.Parameters[0];
            var messageElements = command.Parameters.Except(new List<string>{ receivingUser });

            var message = String.Join(" ", messageElements);

            await SendMessageToServerAsync(new IrcMessage {Command = "PRIVMSG", Parameters = {receivingUser}, TrailingParameter = message});
        }

        /// <summary>
        /// Handler for a user executing a "/join" command. Sends a request to
        /// the server to leave the current channel and join the new one.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task JoinCommandSent(UserCommand command)
        {
            if (command.Parameters.Count == 0) return;

            if (_currentChannel != null)
            {
                await SendMessageToServerAsync(new IrcMessage { Command = "PART", Parameters = { _currentChannel } });
            }

            var channel = command.Parameters[0];
            await SendMessageToServerAsync(new IrcMessage { Command = "JOIN", Parameters = { channel } });
        }

        /// <summary>
        /// Handler for a user executing a "/part" command. Sends a request to the
        /// server to leave the current channel.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task PartCommandSent(UserCommand command)
        {
            if (string.IsNullOrEmpty(_currentChannel)) return;

            TriggerEvent(OnChannelChanged, "");

            await SendMessageToServerAsync(new IrcMessage { Command = "PART", Parameters = { _currentChannel } });

            _currentChannel = null;
        }

        /// <summary>
        /// Handler for a user executing the /nick command.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task NickCommandSent(UserCommand command)
        {
            await SendMessageToServerAsync(new IrcMessage { Command = "NICK", Parameters = command.Parameters});
        }

        /// <summary>
        /// Handler for a user executing the /help command.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task HelpCommandSent(UserCommand command)
        {
            const string joinHelp = "/join <channel> \tJoins a channel";
            const string partHelp = "/part \t\tLeaves the current channel";
            const string langHelp = "/lang \t\tSets the translation language. If 'off', turns translation off";
            const string msgHelp = "/msg <user> <msg> \tSends a private message";
            const string nickHelp = "/nick <nickname> \tChange nickname";
            const string quitHelp = "/quit \t\tDisconnects the client";

            var joinHelpMessage = new Message { Type = MessageType.Server, Text = joinHelp };
            var partHelpMessage = new Message { Type = MessageType.Server, Text = partHelp };
            var langHelpMessage = new Message { Type = MessageType.Server, Text = langHelp };
            var msgHelpMessage = new Message { Type = MessageType.Server, Text = msgHelp };
            var nickHelpMessage = new Message { Type = MessageType.Server, Text = nickHelp };
            var quitHelpMessage = new Message { Type = MessageType.Server, Text = quitHelp };

            TriggerEvent(OnMessageReceived, joinHelpMessage);
            TriggerEvent(OnMessageReceived, partHelpMessage);
            TriggerEvent(OnMessageReceived, langHelpMessage);
            TriggerEvent(OnMessageReceived, msgHelpMessage);
            TriggerEvent(OnMessageReceived, nickHelpMessage);
            TriggerEvent(OnMessageReceived, quitHelpMessage);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for a user executing the /quit command.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        internal virtual async Task QuitCommandSent(UserCommand command)
        {
            await SendMessageToServerAsync(new IrcMessage { Command = "QUIT", TrailingParameter = "Client quit"});
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Handler for any command that is not recognized by the application.
        /// </summary>
        /// <param name="command">The command that was sent by the user</param>
        /// <returns></returns>
        internal virtual async Task UnknownCommandSent(UserCommand command)
        {
            var messageText = string.Format("The command \"/{0}\" is not a known command.", command.Command);
            var message = new Message { Type = MessageType.Server, Text = messageText };

            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Creates and returns a UserCommandHandler instance with the appropriate
        /// event subscriptions.
        /// </summary>
        /// <returns>A fully subscribed UserCommandHandler instance</returns>
        private UserCommandHandler MapUserCommandHandlers()
        {
            var handler = new UserCommandHandler();
            handler.OnJoinCommand += JoinCommandSent;
            handler.OnPartCommand += PartCommandSent;
            handler.OnMessageCommand += PrivateMessageCommandSent;
            handler.OnNickCommand += NickCommandSent;
            handler.OnHelpCommand += HelpCommandSent;
            handler.OnQuitCommand += QuitCommandSent;
            handler.OnUnknownCommand += UnknownCommandSent;
            return handler;
        }
    }
}
