using System;
using System.Threading.Tasks;

namespace IrcClient
{
    public interface IClient
    {
        /// <summary>
        /// Whether or not the client is still connected to the IRC server.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Information about the server the client is currently connected to.
        /// </summary>
        ServerInformation ServerInformation { get; }

        /// <summary>
        /// The current nickname that is registered with the server.
        /// </summary>
        string Nickname { get; }

        /// <summary>
        /// Event handler that fires whenever a message is received from
        /// the server.
        /// </summary>
        event Action<Message> OnMessageReceived;

        /// <summary>
        /// Event handler that fires whenever the client's channel
        /// is changed.
        /// </summary>
        event Action<string> OnChannelChanged;

        /// <summary>
        /// Connects to an IRC server using the information provided.
        /// </summary>
        /// <param name="serverInfo">Information about the server host name and port</param>
        /// <param name="registrationInfo">Information used to register the user with the server</param>
        Task ConnectAsync(ServerInformation serverInfo, RegistrationInformation registrationInfo);

        /// <summary>
        /// Sends a user message to the server asynchronously. Also handles any commands
        /// present in the message, such as /join.
        /// </summary>
        /// <param name="input">The message text to send to the server or execute</param>
        Task SendMessageAsync(string input);

    }
}
