using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrcClient.ClientWrappers;

namespace IrcClient
{
	public partial class Client : IClient
    {

        #region Constructor/Fields

        private readonly ITcpClientWrapper _client;
		private Stream _clientStream;
		private StreamReader _reader;

		private readonly MessageHandler _messageHandler;
		private readonly UserCommandHandler _userCommandHandler;

		private readonly CancellationTokenSource _cancellationTokenSource;

	    public virtual bool IsConnected { get; private set; }

	    public virtual ServerInformation ServerInformation { get; private set; }

		public virtual string Nickname { get; private set; }

		/// <summary>
		/// The current channel the user is connected to.
		/// </summary>
		private string _currentChannel;

		public event Action<Message> OnMessageReceived = delegate { };
		public event Action<string> OnChannelChanged = delegate { };

		public Client(ITcpClientWrapper client)
		{
			_client = client;
			_messageHandler = MapMessageHandlers();
			_userCommandHandler = MapUserCommandHandlers();

			_cancellationTokenSource = new CancellationTokenSource();
		}

        #endregion

        #region Connection

        public async Task ConnectAsync(ServerInformation serverInfo, RegistrationInformation registrationInfo)
		{
			IsConnected = false;
			ServerInformation = serverInfo;
			Nickname = registrationInfo.Nickname;

			_client.Connect(ServerInformation.HostName, ServerInformation.Port);
			await HandleConnectionAsync(registrationInfo);
		}

		/// <summary>
		/// Handles a successful connection to an IRC server. Completes the
		/// connection by sending registration information.
		/// </summary>
		/// <param name="info">The registration info used to register with the server</param>
		internal virtual async Task HandleConnectionAsync(RegistrationInformation info)
		{
			_clientStream = _client.GetStream();
			_reader = new StreamReader(_clientStream, Encoding.Default);

			IsConnected = true;

			await SendRegistrationInfoAsync(info);

            BeginReadAsync();
		}

		/// <summary>
		/// Sends registration information to the server by executing the PASS, NICK,
		/// and USER commands.
		/// </summary>
		/// <param name="info">An object containing the necessary registration information</param>
		internal virtual async Task SendRegistrationInfoAsync(RegistrationInformation info)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info", "The registration info object was null.");
			}

			await SendMessageToServerAsync(new IrcMessage {Command = "PASS", Parameters = {info.Password}});
			await SendMessageToServerAsync(new IrcMessage {Command = "NICK", Parameters = {info.Nickname}});
			await SendMessageToServerAsync(new IrcMessage {Command = "USER", Parameters = {info.Username, "none", "none", info.RealName}});
		}

        #endregion

        #region Read/Write

        /// <summary>
		/// Reads commands from server stream until disconnected.
		/// 
		/// NOTE: I was unable to find a way around that deadlock in the time
		/// I had left so this method has remained async void :(
		/// </summary>
		internal virtual async void BeginReadAsync()
		{
            var token = _cancellationTokenSource.Token;
			while (IsConnected && !token.IsCancellationRequested)
			{
				await ReadAsync();
			}

            IsConnected = false;
		}

		/// <summary>
		/// Reads a single message from the server.
		/// </summary>
		/// <returns></returns>
		internal virtual async Task ReadAsync()
		{
		    var result = await _reader.ReadLineAsync();
			await _messageHandler.ProcessInputAsync(result);
		}

		public async Task SendMessageAsync(string input)
		{
			if (!IsConnected)
			{
				throw new ApplicationException("The client is not connected to a server.");
			}

			if (String.IsNullOrWhiteSpace(input))
			{
				return;
			}

			if (input[0] == '/')
			{
			    await  _userCommandHandler.ProcessInputAsync(input);
			}
			else
			{
				await ProcessUserMessageAsync(input);
			}
		}

		/// <summary>
		/// Processes a message sent by a user.
		/// </summary>
		/// <param name="input">The message text</param>
		/// <returns></returns>
		internal virtual async Task ProcessUserMessageAsync(string input)
		{
			var msg = new IrcMessage { Command = "PRIVMSG", Parameters = {_currentChannel}, TrailingParameter = input };
			await SendMessageToServerAsync(msg);
		}

		/// <summary>
		/// Sends an IrcMessage to the server.
		/// </summary>
		/// <param name="message">The message being sent</param>
		/// <returns></returns>
		internal virtual async Task SendMessageToServerAsync(IrcMessage message)
		{
			var rawMessage = message.ToString();

			var bytes = Encoding.UTF8.GetBytes(rawMessage);
			await _clientStream.WriteAsync(bytes, 0, bytes.Length);
		}

        #endregion

        #region Event

        /// <summary>
		/// Triggers the specified event with the specified parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter the event takes</typeparam>
		/// <param name="eventToTrigger">The event being triggered</param>
		/// <param name="parameter">The parameter to be passed to the subscriber</param>
		internal virtual void TriggerEvent<T>(Action<T> eventToTrigger, T parameter)
		{
			if (eventToTrigger == null)
				return;

            var exceptionList = new List<Exception>();
			foreach (var action in eventToTrigger.GetInvocationList().Cast<Action<T>>())
			{
				try
				{
					action(parameter);
				}
				catch (Exception ex)
				{
				    exceptionList.Add(ex);
				}
			}

            if (exceptionList.Any())
            {
                throw new AggregateException(exceptionList);
            }
        }

        #endregion
    }
}
