using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IrcClient;

namespace ChatClient
{
	public class IrcChatApplication
	{
		private readonly IClient _client;
		private readonly IUserInterface _consoleUserInterface;

		public IrcChatApplication(IClient client, IUserInterface userInterface)
		{
			_client = client;
			_consoleUserInterface = userInterface;
		}

		public async Task Start()
		{
            Console.WindowWidth = 100;
            Console.WindowHeight = 50;

			var serverInfo = ReadServerInfo();

			var registrationInfo = ReadUserRegistrationInfo();

			_client.OnMessageReceived += OutputMessage;
			_client.OnChannelChanged += ChangeChannel;

			try
			{
                _consoleUserInterface.SetupInterface();

                _consoleUserInterface.OutputMessage(new Message { Type = MessageType.Server, Text = "Connecting to server..."});

                await _client.ConnectAsync(serverInfo, registrationInfo);

				_consoleUserInterface.ConsoleTitle = string.Format("{0} > ", _client.ServerInformation.HostName);

				await PromptForInput();
			}
			catch (SocketException)
			{
				Console.WriteLine("ERROR: Unable to connect to {0} on port {1}", _client.ServerInformation.HostName,
					_client.ServerInformation.Port);
			}
			
		}

		/// <summary>
		/// Prompt the user for input and send it using the client. If the user
		/// inputs "/quit", the application will exit.
		/// </summary>
		internal async Task PromptForInput()
		{
			while (true)
			{
				var input = _consoleUserInterface.GetUserInput(_client.Nickname);

				try
				{
					await _client.SendMessageAsync(input);

				    if (input.Equals("/quit", StringComparison.OrdinalIgnoreCase))
				    {
				        break;
				    }
				}
				catch (AggregateException)
				{
					Console.WriteLine("Not connected to a server. Exiting application.");
					break;
				}
			}
		}

		/// <summary>
		/// Output a received message to the console. Subscribes to events
		/// from the client.
		/// </summary>
		/// <param name="message">The message being received</param>
		internal void OutputMessage(Message message)
		{
			if (message != null)
			{
				_consoleUserInterface.OutputMessage(message);
			}
		}

		/// <summary>
		/// Handler for the event of the user joining a channel. Sets the console title.
		/// </summary>
		/// <param name="channel"></param>
		internal void ChangeChannel(string channel)
		{
			_consoleUserInterface.ConsoleTitle = String.Format("{0} > {1}", _client.ServerInformation.HostName, channel);
		}

		/// <summary>
		/// Prompts the user for all of the server information necessary.
		/// </summary>
		/// <returns>An object containing the collected information</returns>
		private static ServerInformation ReadServerInfo()
		{
			Console.Write("Server: ");
			var server = Console.ReadLine();

			Console.Write("Port: ");
			var portString = Console.ReadLine();

			int port;
			var successful = int.TryParse(portString, out port);

			if (!successful)
				return null;

			var info = new ServerInformation
			{
				HostName = server,
				Port = port
			};

			return info;
		}

		/// <summary>
		/// Prompts the user for all of the necessary user registration information.
		/// </summary>
		/// <returns>An object containing the information</returns>
		private static RegistrationInformation ReadUserRegistrationInfo()
		{
			Console.Write("Enter nickname: ");
			var nickname = Console.ReadLine();

			Console.Write("Enter username: ");
			var username = Console.ReadLine();

			Console.Write("Enter real name: ");
			var realName = Console.ReadLine();

			Console.Write("Enter pass: ");
			var pass = Console.ReadLine();

			var info = new RegistrationInformation
			{
				Nickname = nickname,
				Password = pass,
				RealName = realName,
				Username = username
			};

			return info;
		}
	}
}
