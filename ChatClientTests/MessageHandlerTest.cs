using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IrcClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatClientTests
{
    [TestClass]
    public class MessageHandlerTest
    {
        private static List<IrcMessage> _messages; 
        private readonly Func<IrcMessage, Task> _eventFunc = async ircMessage =>
        {
            _messages.Add(ircMessage);
            await Task.Yield();
        };

        [TestInitialize]
        public void SetUp()
        {
            _messages = new List<IrcMessage>();
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_JoinMessage_JoinHandlerCalled()
        {
            const string prefix = "user";
            const string command = "JOIN";
            const string parameter = "#channel";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnJoin += _eventFunc; }, prefix, parameter: parameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(parameter, _messages[0].Parameters[0]);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_NickMessage_NickHandlersCalled()
        {
            const string command = "NICK";
            const string parameter = "newnick";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnNick += _eventFunc; }, parameter:parameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.IsTrue(_messages[0].Parameters.Contains(parameter));
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_PartMessage_PartHandlerCalled()
        {
            const string prefix = "user";
            const string command = "PART";
            const string parameter = "#channel";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnPart += _eventFunc; }, prefix, parameter: parameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(parameter, _messages[0].Parameters[0]);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_PingMessage_PingHandlerCalled()
        {
            const string prefix = "server";
            const string command = "PING";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnPing += _eventFunc; }, prefix).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(0, _messages[0].Parameters.Count);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_Message_MessageHandlerCalled()
        {
            const string prefix = "user";
            const string command = "PRIVMSG";
            const string trailingParameter = "this is a message";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnMessage += _eventFunc; }, prefix, trailingParameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(trailingParameter, _messages[0].TrailingParameter);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_NoticeMessage_NoticeHandlerCalled()
        {
            const string prefix = "server";
            const string command = "NOTICE";
            const string trailingParameter = "a notice";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnNotice += _eventFunc; }, prefix, trailingParameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(trailingParameter, _messages[0].TrailingParameter);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_QuitMessage_QuitHandlerCalled()
        {
            const string prefix = "user!";
            const string command = "QUIT";
            const string trailingParameter = "reason";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(command, handler, () => { handler.OnQuit += _eventFunc; }, prefix, trailingParameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(command, _messages[0].Command);
            Assert.AreEqual(trailingParameter, _messages[0].TrailingParameter);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_NumericCode_NoticeHandlerCalled()
        {
            const string prefix = "server";
            const string trailingParameter = "a notice";
            const string numericCode = "100";

            var handler = new MessageHandler();
            RunProcessInputAsyncMessageTest(numericCode, handler, () => { handler.OnNotice += _eventFunc; }, prefix, trailingParameter).Wait();

            Assert.IsTrue(_messages.Count > 0);
            Assert.AreEqual(prefix, _messages[0].Prefix);
            Assert.AreEqual(numericCode, _messages[0].Command);
            Assert.AreEqual(trailingParameter, _messages[0].TrailingParameter);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_MultipleSubscribers_CallsEach()
        {
            const string message = ":user PRIVMSG :sup";
            var handler = new MessageHandler();
            var firstCount = 0;
            var secondCount = 0;
            handler.OnMessage += x => { firstCount++; return Task.Delay(0); };
            handler.OnMessage += x => { secondCount++; return Task.Delay(0); };

            handler.ProcessInputAsync(message).Wait();
            handler.ProcessInputAsync(message).Wait();

            Assert.AreEqual(2, firstCount);
            Assert.AreEqual(2, secondCount);
        }

        [TestMethod]
        public void ParseMessage_WithPrefix_ParsesSuccessfully()
        {
            const string prefix = "server";
            const string command = "NOTICE";
            var message = string.Format(":{0} {1}", prefix, command);

            var parsedMessage = MessageHandler.ParseIrcMessage(message);

            Assert.AreEqual(prefix, parsedMessage.Prefix);
            Assert.AreEqual(command, parsedMessage.Command);
        }

        [TestMethod]
        public void ParseMessage_WithParameters_ParsesSuccessfully()
        {
            const string command = "test";
            const string firstParameter = "param1";
            const string secondParameter = "param2";
            var message = string.Format("{0} {1} {2}", command, firstParameter, secondParameter);

            var parsedMessage = MessageHandler.ParseIrcMessage(message);

            Assert.AreEqual(command, parsedMessage.Command);
            Assert.AreEqual(2, parsedMessage.Parameters.Count);
            Assert.IsTrue(parsedMessage.Parameters.Contains(firstParameter));
            Assert.IsTrue(parsedMessage.Parameters.Contains(secondParameter));
        }

        [TestMethod]
        public void ParseMessage_WithTrailingParameter_ParsesSuccessfully()
        {
            const string trailingParameter = "trailing........";
            const string command = "NOTICE";
            var message = string.Format("{0} :{1}", command, trailingParameter);

            var parsedMessage = MessageHandler.ParseIrcMessage(message);

            Assert.AreEqual(trailingParameter, parsedMessage.TrailingParameter);
            Assert.AreEqual(command, parsedMessage.Command);
        }

        [TestMethod]
        public void ParseMessage_WithEverything_ParsesSuccessfully()
        {
            const string prefix = "server";
            const string command = "NOTICE";
            const string firstParameter = "param1";
            const string secondParameter = "param2";
            const string trailingParameter = "trailing......";
            var message = string.Format(":{0} {1} {2} {3} :{4}", prefix, command, firstParameter, secondParameter, trailingParameter);

            var parsedMessage = MessageHandler.ParseIrcMessage(message);

            Assert.AreEqual(prefix, parsedMessage.Prefix);
            Assert.AreEqual(command, parsedMessage.Command);
            Assert.AreEqual(2, parsedMessage.Parameters.Count);
            Assert.IsTrue(parsedMessage.Parameters.Contains(firstParameter));
            Assert.IsTrue(parsedMessage.Parameters.Contains(secondParameter));
            Assert.AreEqual(trailingParameter, parsedMessage.TrailingParameter);
        }

        [TestMethod]
        public void ProcessInputAsyncMessage_ExceptionInSubscriber_ThrowsAggregateException()
        {
            const string message = ":user PRIVMSG :sup";
            var handler = new MessageHandler();
            handler.OnMessage += x => { throw new Exception(); };
            handler.OnMessage += x => { throw new Exception(); };

            try
            {
                handler.ProcessInputAsync(message).Wait();
                Assert.Fail("Should have thrown an AggregateException");
            }
            catch (AggregateException ex)
            {
                var aggregateException = ex.InnerExceptions[0] as AggregateException;
                Assert.IsNotNull(aggregateException);
                Assert.AreEqual(2, aggregateException.InnerExceptions.Count);
            }
        }

        private static async Task RunProcessInputAsyncMessageTest(string command, IrcEventHandler handler, Action handlerFunc, 
            string prefix = null, string trailingParameter = null, string parameter = null)
        {
            var builder = new StringBuilder();

            if (prefix != null)
            {
                builder.Append(string.Format(":{0} ", prefix));
            }

            builder.Append(string.Format("{0}", command));

            if (parameter != null)
            {
                builder.Append(string.Format(" {0}", parameter));
            }

            if (trailingParameter != null)
            {
                builder.Append(string.Format(" :{0}", trailingParameter));
            }

            var rawCommand = builder.ToString();

            handlerFunc();

            await handler.ProcessInputAsync(rawCommand);
        }
    }
}
