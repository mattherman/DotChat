using System;
using System.Linq;
using System.Threading.Tasks;

namespace IrcClient
{
    public class UserCommandHandler : IrcEventHandler
    {
        public event Func<UserCommand, Task> OnJoinCommand;
        public event Func<UserCommand, Task> OnPartCommand;
        public event Func<UserCommand, Task> OnMessageCommand;
        public event Func<UserCommand, Task> OnNickCommand; 
        public event Func<UserCommand, Task> OnHelpCommand;
        public event Func<UserCommand, Task> OnQuitCommand; 
        public event Func<UserCommand, Task> OnUnknownCommand;
 
        public override async Task ProcessInputAsync(string rawCommand)
        {
            var command = ParseUserCommand(rawCommand);

            switch (command.Command.ToUpper())
            {
                case "JOIN":
                    await TriggerEventAsync(OnJoinCommand, command);
                    break;
                case "PART":
                    await TriggerEventAsync(OnPartCommand, command);
                    break;
                case "MSG":
                    await TriggerEventAsync(OnMessageCommand, command);
                    break;
                case "NICK":
                    await TriggerEventAsync(OnNickCommand, command);
                    break;
                case "HELP":
                    await TriggerEventAsync(OnHelpCommand, command);
                    break;
                case "QUIT":
                    await TriggerEventAsync(OnQuitCommand, command);
                    break;
                default:
                    await TriggerEventAsync(OnUnknownCommand, command);
                    break;
            }
        }

        /// <summary>
        /// Parses the input into a command and a set of parameters if any exist.
        /// </summary>
        /// <param name="rawCommand">The raw input to be parsed</param>
        /// <returns>A UserCommand including the command and parameters</returns>
        internal UserCommand ParseUserCommand(string rawCommand)
        {
            var commandAndParams = rawCommand.Split(' ');
            var command = commandAndParams[0].Substring(1);

            var userCommand = new UserCommand
            {
                Command = command,
                Parameters = commandAndParams.Skip(1).Take(commandAndParams.Length - 1).ToArray()
            };

            return userCommand;
        }
    }
}
