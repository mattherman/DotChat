using System.Threading.Tasks;
using ChatClient;
using IrcClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ChatClientTests
{
	[TestClass]
	public class IrcChatApplicationTest
	{
		private readonly Mock<IClient> _mockClient = new Mock<IClient>(MockBehavior.Strict);
		private readonly Mock<IUserInterface> _mockInterface = new Mock<IUserInterface>(MockBehavior.Strict);

		private IrcChatApplication _application;

		[TestInitialize]
		public void TestSetup()
		{
			_application = new IrcChatApplication(_mockClient.Object, _mockInterface.Object);
		}

		[TestCleanup]
		public void VerifyMocks()
		{
			_mockClient.Verify();
			_mockInterface.Verify();
		}

		[TestMethod]
		public void PromptForInput_ReceivesInputUntilQuit()
		{
			const string nickname = "jsmith12315235";
			_mockInterface.SetupSequence(i => i.GetUserInput(nickname)).Returns("some input").Returns("/quit");
			_mockClient.SetupGet(c => c.Nickname).Returns(nickname);
			_mockClient.Setup(c => c.SendMessageAsync(It.IsAny<string>())).Returns(Task.Delay(0)).Verifiable();

			_application.PromptForInput().Wait();
		}

		[TestMethod]
		public void OutputMessage_NullMessage_DoesNothing()
		{
			_application.OutputMessage(null);
			_mockInterface.Verify(i => i.OutputMessage(It.IsAny<Message>()), Times.Never);
		}

		[TestMethod]
		public void OutputMessage_ValidMessage_SendsToUserInterface()
		{
			var message = new Message {Text = "Hello, World!"};
			_mockInterface.Setup(i => i.OutputMessage(message)).Verifiable();

			_application.OutputMessage(message);
		}

		[TestMethod]
		public void ChangeChannel_SetsUserInterfaceTitle()
		{
			const string channel = "#mychannel";
			const string hostname = "localhost";
			var serverInfo = new ServerInformation {HostName = hostname};
			_mockClient.SetupGet(c => c.ServerInformation).Returns(serverInfo);
			_mockInterface.SetupSet(i => i.ConsoleTitle = It.Is<string>(t => t.Contains(hostname) && t.Contains(channel))).Verifiable();

			_application.ChangeChannel(channel);
		}

	}
}
