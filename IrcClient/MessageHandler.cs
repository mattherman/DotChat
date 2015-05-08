using System;
using System.Linq;
using System.Threading.Tasks;

namespace IrcClient
{

    public class MessageHandler : IrcEventHandler
    {

        public event Func<IrcMessage, Task> OnNotice;
        public event Func<IrcMessage, Task> OnMessage;
        public event Func<IrcMessage, Task> OnJoin; 
        public event Func<IrcMessage, Task> OnPart;
        public event Func<IrcMessage, Task> OnNick; 
        public event Func<IrcMessage, Task> OnPing;
        public event Func<IrcMessage, Task> OnQuit; 

        public override async Task ProcessInputAsync(string rawMessage)
        {
            rawMessage = rawMessage.TrimEnd('\r', '\n', '\0');
            var message = ParseIrcMessage(rawMessage);

            switch (message.Command.ToUpper())
            {
                case "NOTICE":
                    await TriggerEventAsync(OnNotice, message);
                    break;
                case "PRIVMSG":
                    await TriggerEventAsync(OnMessage, message);
                    break;
                case "JOIN":
                    await TriggerEventAsync(OnJoin, message);
                    break;
                case "NICK":
                    await TriggerEventAsync(OnNick, message);
                    break;
                case "PART":
                    await TriggerEventAsync(OnPart, message);
                    break;
                case "PING":
                    await TriggerEventAsync(OnPing, message);
                    break;
                case "QUIT":
                    await TriggerEventAsync(OnQuit, message);
                    break;
                default:
                    int num;
                    if (int.TryParse(message.Command, out num))
                    {
                        // Numeric codes should be treated as a notice
                        // They are either basically notices, or errors that have
                        // their own error messages, no reason to handle separately
                        await TriggerEventAsync(OnNotice, message);    
                    }
                    break;
            }
        }

        /// <summary>
        /// Parses the different pieces of an IRC message from a raw message. The format is as follows:
        /// 
        /// :PREFIX COMMAND PARAMETERS :TRAILING
        /// 
        /// All of the elements of the message are optional, except for the command. They must occur
        /// in the given order when they are present. The :TRAILING element is the only one that can
        /// contain spaces.
        /// 
        /// Source: http://calebdelnay.com/blog/2010/11/parsing-the-irc-message-format-as-a-client
        /// 
        /// </summary>
        /// <param name="message">The raw message that is being parsed.</param>
        internal static IrcMessage ParseIrcMessage(string message)
        {
            var prefixEnd = -1;
            string trailing = null;
            var prefix = String.Empty;
            string[] parameters = { };

            if (message.StartsWith(":"))
            {
                prefixEnd = message.IndexOf(" ", StringComparison.Ordinal);
                prefix = message.Substring(1, prefixEnd - 1);
            }

            var trailingStart = message.IndexOf(" :", StringComparison.Ordinal);
            if (trailingStart >= 0)
                trailing = message.Substring(trailingStart + 2);
            else
                trailingStart = message.Length;

            var commandAndParameters = message.Substring(prefixEnd + 1, trailingStart - prefixEnd - 1).Split(' ');

            var command = commandAndParameters.First();

            if (commandAndParameters.Length > 1)
                parameters = commandAndParameters.Skip(1).ToArray();

            trailing = trailing ?? "";

            return new IrcMessage { Prefix = prefix, Command = command, Parameters = parameters, TrailingParameter = trailing };
        }
    }
}
