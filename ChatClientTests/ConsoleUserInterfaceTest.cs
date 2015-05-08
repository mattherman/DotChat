using ChatClient;
using IrcClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatClientTests
{
	[TestClass]
	public class ConsoleUserInterfaceTest
	{

		[TestMethod]
		public void GetFormattedMessage_ServerMessage()
		{
			const string message = "Notice from server";

			var result = ConsoleUserInterface.GetFormattedMessage(message, string.Empty, MessageType.Server);

			Assert.IsTrue(result.Contains(string.Format("== {0}", message)));
		}

		[TestMethod]
		public void GetFormattedMessage_UserMessage()
		{
			const string message = "Notice from server";
			const string user = "jsmith1858344";

			var result = ConsoleUserInterface.GetFormattedMessage(message, user, MessageType.User);

			Assert.IsTrue(result.Contains(string.Format("<{0}> {1}", user, message)));
		}

	    [TestMethod]
	    public void GetFormattedMessage_PrivateMessage()
	    {
	        const string message = "What's up";
	        const string user = "jsmith983822";

	        var result = ConsoleUserInterface.GetFormattedMessage(message, user, MessageType.Private);

            Assert.IsTrue(result.Contains(string.Format("*{0}* {1}", user, message)));
	    }
	}
}
