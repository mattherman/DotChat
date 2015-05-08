using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcClient;
using IrcClient.ClientWrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ChatClientTests
{
	[TestClass]
	public class ClientTest
	{
		#region Test Setup

		private readonly Mock<ITcpClientWrapper> _mockTcpClient = new Mock<ITcpClientWrapper>(MockBehavior.Strict);
		private Mock<Client> _mockClient;
		private Client _client;

		[TestInitialize]
		public void SetupMocks()
		{
			_mockClient = new Mock<Client>(_mockTcpClient.Object) { CallBase = true };
			_client = _mockClient.Object;
		}

		[TestCleanup]
		public void VerifyMocks()
		{
			_mockTcpClient.Verify();
			_mockClient.Verify();
		}

		#endregion

		#region SendRegistrationInfoAsync

		[TestMethod]
		public void SendRegistrationInfoAsync_NullArgument_ThrowsNullArgumentException()
		{
			try
			{
				_client.SendRegistrationInfoAsync(null).Wait();
				Assert.Fail("Should have thrown an AggregateException.");
			}
			catch (AggregateException ex)
			{
				Assert.AreEqual(1, ex.InnerExceptions.Count);
				Assert.AreEqual(typeof(ArgumentNullException), ex.InnerExceptions.First().GetType());
			}
		}

		[TestMethod]
		public void SendRegistrationInfoAsync_ValidInfo_SendsMessagesToServer()
		{
			const string password = "Pa$$word123";
			const string nickname = "iluvcats92";
			const string user = "jsmith";
			const string realName = "John Smith";

			var registrationInfo = new RegistrationInformation
			{
				Nickname = nickname,
				Password = password,
				Username = user,
				RealName = realName
			};

			_mockClient.Setup(
				f =>
					f.SendMessageToServerAsync(It.Is<IrcMessage>(m => m.Command == "PASS" && m.Parameters.Contains(password))))
					.Returns(Task.Delay(0)).Verifiable();

			_mockClient.Setup(
				f =>
					f.SendMessageToServerAsync(It.Is<IrcMessage>(m => m.Command == "NICK" && m.Parameters.Contains(nickname))))
					.Returns(Task.Delay(0)).Verifiable();

			_mockClient.Setup(
				f =>
					f.SendMessageToServerAsync(It.Is<IrcMessage>(m => m.Command == "USER" && m.Parameters.Contains(user) && m.Parameters.Contains(realName))))
					.Returns(Task.Delay(0)).Verifiable();

			_client.SendRegistrationInfoAsync(registrationInfo).Wait();
		}

		#endregion

		#region SendMessageAsync

		[TestMethod]
		public void SendMessageAsync_NoConnection_ThrowsApplicationException()
		{
			try
			{
				_client.SendMessageAsync("test").Wait();
				Assert.Fail("Expected an AggregateException to be thrown from the asynchronous method.");
			}
			catch (AggregateException ex)
			{
				Assert.AreEqual(1, ex.InnerExceptions.Count);
				Assert.AreEqual(typeof(ApplicationException), ex.InnerExceptions.First().GetType());
			}
		}

		[TestMethod]
		public void SendMessageAsync_EmptyInput_DoesNothing()
		{
		    _mockClient.SetupGet(c => c.IsConnected).Returns(true);

			_client.SendMessageAsync(string.Empty).Wait();
			_mockClient.Verify(f => f.ProcessUserMessageAsync(It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void SendMessageAsync_NullInput_DoesNothing()
		{
            _mockClient.SetupGet(c => c.IsConnected).Returns(true);

			_client.SendMessageAsync(null).Wait();
			_mockClient.Verify(f => f.ProcessUserMessageAsync(It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void SendMessageAsync_UserCommand_CallsCommandHandler()
		{
			const string command = "JOIN";
			const string channel = "#mychannel";
			var input = string.Format("/{0} {1}", command, channel);

            _mockClient.SetupGet(c => c.IsConnected).Returns(true);

			_mockClient.Setup(f => f.JoinCommandSent(
				It.Is<UserCommand>(c => c.Command == command && c.Parameters.Contains(channel))))
				.Returns(Task.Delay(0)).Verifiable();

			_client.SendMessageAsync(input).Wait();
		}

		[TestMethod]
		public void SendMessageAsync_Message_SendsMessage()
		{
			const string message = "Hello, World!";

            _mockClient.SetupGet(c => c.IsConnected).Returns(true);
			_mockClient.Setup(f => f.ProcessUserMessageAsync(message)).Returns(Task.Delay(0)).Verifiable();

			_client.SendMessageAsync(message).Wait();
		}

		#endregion

		#region ProcessUserMessageAsync

		[TestMethod]
		public void ProcessUserMessageAsync_SendsPrivateMessageToServer()
		{
			const string message = "Hello, World!";
			_mockClient.Setup(f => f.SendMessageToServerAsync(It.Is<IrcMessage>(m => m.Command == "PRIVMSG" && m.TrailingParameter == message)))
				.Returns(Task.Delay(0))
				.Verifiable();

			_client.ProcessUserMessageAsync(message).Wait();
		}

		#endregion

		#region Connection Methods

		[TestMethod]
		public void ConnectAsync_Successful_ConnectsClientAndSetsProperties()
		{
			const string hostname = "localhost";
			const int port = 6667;
			const string nickname = "hax2themax";

			var serverInfo = new ServerInformation {HostName = hostname, Port = port};
			var registrationInfo = new RegistrationInformation {Nickname = nickname};

			_mockTcpClient.Setup(tcp => tcp.Connect(hostname, port)).Verifiable();

			_mockClient.Setup(f => f.HandleConnectionAsync(registrationInfo)).Returns(Task.Delay(0)).Verifiable();

			_client.ConnectAsync(serverInfo, registrationInfo).Wait();

			Assert.AreEqual(serverInfo, _client.ServerInformation);
			Assert.AreEqual(nickname, _client.Nickname);
		}

		[TestMethod]
		public void HandleConnectionAsync_Successful_RegistersAndReads()
		{
			var registrationInfo = new RegistrationInformation {Nickname = "hax2themax"};

			_mockTcpClient.Setup(tcp => tcp.GetStream()).Returns(new MemoryStream());
			_mockClient.Setup(f => f.SendRegistrationInfoAsync(registrationInfo)).Returns(Task.Delay(0)).Verifiable();
			_mockClient.Setup(f => f.BeginReadAsync()).Verifiable();

			_client.HandleConnectionAsync(registrationInfo).Wait();
		}

		#endregion

		#region Read Methods

		[TestMethod]
		public void BeginReadAsync_TokenCancellationRequested_StopsReading()
		{
			_mockClient.Setup(f => f.SendMessageToServerAsync(It.IsAny<IrcMessage>())).Returns(Task.Delay(0));

			_mockClient.Setup(f => f.ReadAsync()).Callback(() => _client.SendMessageAsync("/quit").Wait()).Returns(Task.Delay(0));

			_mockTcpClient.Setup(tcp => tcp.GetStream()).Returns(new MemoryStream());
			_mockClient.Setup(f => f.SendRegistrationInfoAsync(It.IsAny<RegistrationInformation>()))
				.Returns(Task.Delay(0)).Verifiable();

			_client.HandleConnectionAsync(null).Wait();

			_mockClient.Verify(f => f.ReadAsync(), Times.Once);
		}

		private void MockStreamData(string data)
		{
			var bytes = Encoding.Default.GetBytes(data);
			_mockTcpClient.Setup(tcp => tcp.GetStream()).Returns(new MemoryStream(bytes));
			_mockClient.Setup(f => f.SendRegistrationInfoAsync(It.IsAny<RegistrationInformation>()))
				.Returns(Task.Delay(0)).Verifiable();
			_mockClient.Setup(f => f.BeginReadAsync()).Verifiable();
			_client.HandleConnectionAsync(null).Wait();
		}

		[TestMethod]
		public void ReadAsync_CommandReceived_CallsHandler()
		{
			const string message = "JOIN #channel";
            MockStreamData(message);
			_mockClient.Setup(f => f.JoinReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

			_client.ReadAsync().Wait();
		}

		[TestMethod]
		public void ReadAsync_PartReceived_CallsPartHandler()
		{
			const string message = "PART #channel";
            MockStreamData(message);
			_mockClient.Setup(f => f.PartReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

			_client.ReadAsync().Wait();
		}

		[TestMethod]
		public void ReadAsync_MessageReceived_CallsMessageHandler()
		{
			const string message = "PRIVMSG #channel :hello";
            MockStreamData(message);
			_mockClient.Setup(f => f.MessageReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

			_client.ReadAsync().Wait();
		}

		[TestMethod]
		public void ReadAsync_NoticeReceived_CallsNoticeHandler()
		{
			const string message = "NOTICE :hello";
            MockStreamData(message);
			_mockClient.Setup(f => f.NoticeReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

			_client.ReadAsync().Wait();
		}

		[TestMethod]
		public void ReadAsync_PingReceived_CallsPingHandler()
		{
			const string message = "PING";
            MockStreamData(message);
			_mockClient.Setup(f => f.PingReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

			_client.ReadAsync().Wait();
		}

	    [TestMethod]
	    public void ReadAsync_QuitReceived_CallsQuitHandler()
	    {
	        const string message = "QUIT";
	        MockStreamData(message);
            _mockClient.Setup(f => f.QuitReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

	        _client.ReadAsync().Wait();
	    }

	    [TestMethod]
	    public void ReadAsync_NickReceived_CallsNickHandler()
	    {
	        const string message = ":oldNick! NICK :newNick";
	        MockStreamData(message);
            _mockClient.Setup(f => f.NickReceived(It.IsAny<IrcMessage>())).Returns(Task.Delay(0)).Verifiable();

	        _client.ReadAsync().Wait();
	    }

		#endregion

		#region ParseUserFromPrefix

		[TestMethod]
		public void ParseUserFromPrefix_NoUserInPrefix_ReturnsUnknown()
		{
			const string expectedResult = "unknown";
			var result = Client.ParseUserFromPrefix("gibberish");
			Assert.AreEqual(expectedResult, result);
		}

		[TestMethod]
		public void ParseUserFromPrefix_ReturnsUser()
		{
			const string expectedResult = "hax0r4lyfe";
			var prefix = string.Format("{0}!", expectedResult);
			var result = Client.ParseUserFromPrefix(prefix);
			Assert.AreEqual(expectedResult, result);
		}

		#endregion

		#region Handlers

		[TestMethod]
		public void NoticeReceived_TriggersMessageEvent()
		{
			const string noticeText = "hello";
			var ircMessage = new IrcMessage {TrailingParameter = noticeText};

			_mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(),
                It.Is<Message>(m => m.Text == noticeText && m.Type == MessageType.Server))).Verifiable();

			_client.NoticeReceived(ircMessage).Wait();
		}

		[TestMethod]
		public void MessageReceived_FromChannel_TriggersMessageEventWithUserMessage()
		{
			const string messageText = "hello";
			const string user = "jsmith194783";
			var ircMessage = new IrcMessage
			                 {
				                 Command = "PRIVMSG", TrailingParameter = messageText, Prefix = string.Format("{0}!", user)
			                 };

			_mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(), 
				It.Is<Message>(m => m.Text == messageText && m.User == user && m.Type == MessageType.User))).Verifiable();

			_client.MessageReceived(ircMessage).Wait();
		}

	    [TestMethod]
	    public void MessageReceived_FromUser_TriggersMessageEventWithPrivateMessage()
	    {
            const string messageText = "hello";
            const string user = "anotherUser";
	        const string nickname = "jsmith923984";
            var ircMessage = new IrcMessage
            {
                Command = "PRIVMSG",
                TrailingParameter = messageText,
                Prefix = string.Format("{0}!", user),
                Parameters = { nickname }
            };

	        _mockClient.SetupGet(f => f.Nickname).Returns(nickname);
            _mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(),
                It.Is<Message>(m => m.Text == messageText && m.User == user && m.Type == MessageType.Private))).Verifiable();

            _client.MessageReceived(ircMessage).Wait();
	    }

		[TestMethod]
		public void JoinReceived_WithParameters_TriggersChannelAndMessageEvent()
		{
			const string channel = "#channel";
			const string user = "jsmith38482";
			var ircMessage = new IrcMessage { Command = "JOIN", Prefix = string.Format("{0}!", user), Parameters = { channel } };

		    _mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<string>>(), channel)).Verifiable();
			_mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(),
				It.Is<Message>(m => m.Text.Contains(channel) && m.Text.Contains(user) && m.Type == MessageType.Server))).Verifiable();

		    _client.JoinReceived(ircMessage).Wait();
		}

		[TestMethod]
		public void JoinReceived_NoParameters_DoesNothing()
		{
			_client.JoinReceived(new IrcMessage {Command = "JOIN"}).Wait();
			_mockClient.Verify(f => f.TriggerEvent(It.IsAny<Action<string>>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void PartReceived_WithParameters_TriggersMessageEvent()
		{
			const string channel = "#channel";
			const string user = "jsmith234236";
			var ircMessage = new IrcMessage {Command = "PART", Prefix = string.Format("{0}!", user), Parameters = {channel}};

			_mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(),
                It.Is<Message>(m => m.Text.Contains(channel) && m.Text.Contains(user) && m.Type == MessageType.Server))).Verifiable();

			_client.PartReceived(ircMessage).Wait();
		}

		[TestMethod]
		public void PartReceived_NoParameters_DoesNothing()
		{
			_client.JoinReceived(new IrcMessage { Command = "PART" }).Wait();
			_mockClient.Verify(f => f.TriggerEvent(It.IsAny<Action<Message>>(), It.IsAny<Message>()), Times.Never);
		}

		[TestMethod]
		public void PingReceived_SendsPong()
		{
			_mockClient.Setup(f => f.SendMessageToServerAsync(It.Is<IrcMessage>(m => m.Command == "PONG")))
				.Returns(Task.Delay(0)).Verifiable();

			_client.PingReceived(new IrcMessage {Command = "PING"}).Wait();
		}

	    [TestMethod]
	    public void NickReceived_DifferentUser_SendsMessage()
	    {
	        const string oldNick = "jsmith3823892";
	        const string newNick = "jsmith0028482";

            _mockClient.Setup(
                f => 
                    f.TriggerEvent(It.IsAny<Action<Message>>(), 
                        It.Is<Message>(m => m.Text.Contains(oldNick) && m.Text.Contains(newNick) && m.Type == MessageType.Server)))
                            .Verifiable();

	        _client.NickReceived(new IrcMessage { Command = "NICK", Prefix = string.Format("{0}!", oldNick), TrailingParameter = newNick}).Wait();
	    }

	    [TestMethod]
	    public void NickReceived_SameUser_ChangesNicknameAndSendsMessage()
	    {
            const string oldNick = "jsmith3823892";
            const string newNick = "jsmith0028482";

	        _mockClient.SetupGet(c => c.Nickname).Returns(oldNick);
            _mockClient.Setup(
                f =>
                    f.TriggerEvent(It.IsAny<Action<Message>>(),
                        It.Is<Message>(m => m.Text.Contains(oldNick) && m.Text.Contains(newNick) && m.Type == MessageType.Server)))
                            .Verifiable();

            _client.NickReceived(new IrcMessage { Command = "NICK", Prefix = string.Format("{0}!", oldNick), TrailingParameter = newNick }).Wait();
	    }

	    [TestMethod]
	    public void QuitReceived_UserQuit_DoesNothing()
	    {
	        const string nickname = "jsmith388483";
	        _mockClient.SetupGet(c => c.Nickname).Returns(nickname);

	        _client.QuitReceived(new IrcMessage {Command = "QUIT", Prefix = string.Format("{0}!", nickname)}).Wait();

            _mockClient.Verify(f => f.TriggerEvent(It.IsAny<Action<Message>>(), It.IsAny<Message>()), Times.Never);
	    }

        [TestMethod]
	    public void QuitReceived_SendsMessageWithUserAndReason()
	    {
	        const string reason = "Client quit";
	        const string user = "jsmith920394";

	        _mockClient.Setup(
	            f =>
	                f.TriggerEvent(It.IsAny<Action<Message>>(),
	                    It.Is<Message>(m => m.Text.Contains(reason) && m.Text.Contains(user)))).Verifiable();

            _client.QuitReceived(new IrcMessage { Command = "QUIT", Prefix = string.Format("{0}!", user), TrailingParameter = reason}).Wait();
	    }

		[TestMethod]
		public void JoinSent_WithNoChannelAndWithChannel_JoinsNewChannelPartsOldOne()
		{
			// No current channel
			const string joinCommand = "JOIN";
			const string channel = "#mychannel";
			var command = new UserCommand {Command = joinCommand, Parameters = {channel}};

			_mockClient.Setup(
				f => f.SendMessageToServerAsync(
					It.Is<IrcMessage>(m => m.Command == joinCommand && m.Parameters.Contains(channel))))
					.Returns(Task.Delay(0)).Verifiable();

			_client.JoinCommandSent(command).Wait();

			// Receive channel change confirmation from "server"
			_client.JoinReceived(new IrcMessage { Command = "JOIN", Parameters = { channel } }).Wait();

			// With current channel set
			const string partCommand = "PART";
			const string newChannel = "#myotherchannel";
			command = new UserCommand {Command = joinCommand, Parameters = {newChannel}};

			_mockClient.Setup(
				f => f.SendMessageToServerAsync(
					It.Is<IrcMessage>(m => m.Command == partCommand && m.Parameters.Contains(channel))))
					.Returns(Task.Delay(0)).Verifiable();

			_mockClient.Setup(
				f => f.SendMessageToServerAsync(
					It.Is<IrcMessage>(m => m.Command == joinCommand && m.Parameters.Contains(newChannel))))
					.Returns(Task.Delay(0)).Verifiable();

			_client.JoinCommandSent(command).Wait();
		}

		[TestMethod]
		public void JoinSent_NoParameters_DoesNothing()
		{
			_client.JoinCommandSent(new UserCommand { Command = "JOIN" }).Wait();
			_mockClient.Verify(f => f.SendMessageToServerAsync(It.IsAny<IrcMessage>()), Times.Never);
		}

		[TestMethod]
		public void PartSent_WithChannelAndWithoutChannel_SendsMessageAndChannelChange()
		{
			const string currentChannel = "#mychannel";
			const string partCommand = "PART";

			// Receive channel change confirmation from "server"
			_client.JoinReceived(new IrcMessage {Command = "JOIN", Parameters = {currentChannel}}).Wait();

			// Send part command with channel set
			var command = new UserCommand {Command = partCommand};

			_mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<string>>(), string.Empty)).Verifiable();
			_mockClient.Setup(f => f.SendMessageToServerAsync(
				It.Is<IrcMessage>(m => m.Command == partCommand && m.Parameters.Contains(currentChannel))))
				.Returns(Task.Delay(0)).Verifiable();

			_client.PartCommandSent(command).Wait();

			// Send part command without channel set
			_client.PartCommandSent(command).Wait();

			_mockClient.Verify(f => f.TriggerEvent(It.IsAny<Action<string>>(), string.Empty), Times.Once);
		}

	    [TestMethod]
	    public void HelpSent_RepliesWithHelpMessages()
	    {
	        var command = new UserCommand {Command = "HELP"};

            _mockClient.Setup(f => f.TriggerEvent(It.IsAny<Action<Message>>(), It.Is<Message>(m => m.Type == MessageType.Server)))
	            .Verifiable();

	        _client.HelpCommandSent(command).Wait();

            _mockClient.Verify(f => f.TriggerEvent(It.IsAny<Action<Message>>(), It.Is<Message>(m => m.Type == MessageType.Server)), Times.Exactly(6));
	    }

	    [TestMethod]
	    public void QuitSent_SendsMessageToServerWithReason()
	    {
	        var command = new UserCommand {Command = "QUIT"};

            _mockClient.Setup(f => f.SendMessageToServerAsync(
                It.Is<IrcMessage>(m => m.Command == command.Command && m.TrailingParameter == "Client quit")))
                .Returns(Task.Delay(0)).Verifiable();

	        _client.QuitCommandSent(command).Wait();
	    }

	    [TestMethod]
	    public void NickSent_SendsMessageToServer()
	    {
	        const string nickname = "jsmith30239";
	        var command = new UserCommand {Command = "NICK", Parameters = {nickname}};

            _mockClient.Setup(f => f.SendMessageToServerAsync(
                It.Is<IrcMessage>(m => m.Command == command.Command && m.Parameters.Contains(nickname))))
                .Returns(Task.Delay(0)).Verifiable();

	        _client.NickCommandSent(command).Wait();
	    }

	    [TestMethod]
	    public void UnknownCommandSent_RepliesWithMessage()
	    {
	        var command = new UserCommand {Command = "jlfi3nfsal"};

	        _mockClient.Setup(
	            f =>
	                f.TriggerEvent(It.IsAny<Action<Message>>(),
                        It.Is<Message>(m => m.Type == MessageType.Server && m.Text.Contains(command.Command)))).Verifiable();

	        _client.UnknownCommandSent(command).Wait();
	    }

		#endregion
	}
}
