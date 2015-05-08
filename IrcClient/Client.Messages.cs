using System.Threading.Tasks;

namespace IrcClient
{
    public partial class Client
    {

        /// <summary>
        /// Handler for receiving NOTICE messages from the server.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task NoticeReceived(IrcMessage ircMessage)
        {
            var message = new Message { Text = ircMessage.TrailingParameter, Type = MessageType.Server };
            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for receiving PRIVMSG messages from the server.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task MessageReceived(IrcMessage ircMessage)
        {
            var user = ParseUserFromPrefix(ircMessage.Prefix);

            var messageText = ircMessage.TrailingParameter;

            var type = MessageType.User;
            if (ircMessage.Parameters.Count > 0 && ircMessage.Parameters[0] == Nickname)
            {
                type = MessageType.Private;
            }

            var message = new Message { Text = messageText, User = user, Type = type };
            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for receiving NICK messages from the server. Updates the user's 
        /// nickname if they are the one who changed their nickname.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task NickReceived(IrcMessage ircMessage)
        {
            var previousNick = ParseUserFromPrefix(ircMessage.Prefix);
            var newNick = ircMessage.TrailingParameter;

            if (previousNick == Nickname)
            {
                Nickname = newNick;
            }

            var messageText = string.Format("{0} is now known as {1}", previousNick, newNick);
            var message = new Message {Text = messageText, Type = MessageType.Server};
            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for receiving JOIN messages from the server. Updates the user's
        /// current channel.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task JoinReceived(IrcMessage ircMessage)
        {
            if (ircMessage.Parameters.Count == 0) return;

            var user = ParseUserFromPrefix(ircMessage.Prefix);

            var channel = ircMessage.Parameters[0];
            _currentChannel = channel;

            TriggerEvent(OnChannelChanged, _currentChannel);

            var messageText = string.Format("{0} has joined {1}", user, _currentChannel);

            var message = new Message { Text = messageText, Type = MessageType.Server };
            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for receiving PART messages from the server. Updates the user's
        /// current channel.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task PartReceived(IrcMessage ircMessage)
        {
            if (ircMessage.Parameters.Count == 0) return;

            var user = ParseUserFromPrefix(ircMessage.Prefix);

            var messageText = string.Format("{0} has left {1}", user, ircMessage.Parameters[0]);

            var message = new Message { Text = messageText, Type = MessageType.Server };
            TriggerEvent(OnMessageReceived, message);

            await Task.Yield();
        }

        /// <summary>
        /// Handler for receiving PING messages from the server. Responds with a
        /// PONG message.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task PingReceived(IrcMessage ircMessage)
        {
            await SendMessageToServerAsync(new IrcMessage { Command = "PONG" });
        }

        /// <summary>
        /// Handler for receiving QUIT messages from the server. If the message is
        /// for the current user, it does nothing.
        /// </summary>
        /// <param name="ircMessage">The message received</param>
        internal virtual async Task QuitReceived(IrcMessage ircMessage)
        {
            var user = ParseUserFromPrefix(ircMessage.Prefix);

            if (user == Nickname)
                return;

            var reason = ircMessage.TrailingParameter;
            var text = string.Format("User {0} has quit [{1}]", user, reason);

            TriggerEvent(OnMessageReceived, new Message { Type = MessageType.Server, Text = text });

            await Task.Yield();
        }

        /// <summary>
        /// Parses a username from a received messages prefix.
        /// </summary>
        /// <param name="prefix">The prefix of the message received</param>
        /// <returns>The username of the user the message pertains to</returns>
        internal static string ParseUserFromPrefix(string prefix)
        {
            var prefixParts = prefix.Split('!');
            return prefixParts.Length > 1 ? prefixParts[0] : "unknown";
        }

        /// <summary>
        /// Creates and returns a MessageHandler instance with the appropriate
        /// event subscriptions.
        /// </summary>
        /// <returns>A fully subscribed MessageHandler isntance</returns>
        private MessageHandler MapMessageHandlers()
        {
            var handler = new MessageHandler();
            handler.OnMessage += MessageReceived;
            handler.OnNotice += NoticeReceived;
            handler.OnJoin += JoinReceived;
            handler.OnPart += PartReceived;
            handler.OnNick += NickReceived;
            handler.OnPing += PingReceived;
            handler.OnQuit += QuitReceived;
            return handler;
        }
    }
}
