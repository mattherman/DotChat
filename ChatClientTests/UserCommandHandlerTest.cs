using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IrcClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatClientTests
{
    [TestClass]
    public class UserCommandHandlerTest
    {
        private static List<UserCommand> _commands;
        private readonly Func<UserCommand, Task> _eventFunc = async ircMessage =>
        {
            _commands.Add(ircMessage);
            await Task.Yield();
        };

        [TestInitialize]
        public void SetUp()
        {
            _commands = new List<UserCommand>();
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_JoinCommand_JoinHandlerCalled()
        {
            const string command = "join";
            const string parameter = "#channel";
            var handler = new UserCommandHandler();
            
            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnJoinCommand += _eventFunc; }, parameter).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count > 0);
            Assert.AreEqual(parameter, _commands[0].Parameters[0]);
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_PartCommand_JoinHandlerCalled()
        {
            const string command = "part";
            var handler = new UserCommandHandler();
            
            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnPartCommand += _eventFunc; }).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count == 0);
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_NickCommand_NickHandlerCalled()
        {
            const string command = "nick";
            const string parameter = "newnickname";
            var handler = new UserCommandHandler();

            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnNickCommand += _eventFunc; }, parameter).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count > 0);
            Assert.AreEqual(parameter, _commands[0].Parameters[0]);
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_QuitCommand_QuitHandlerCalled()
        {
            const string command = "quit";
            var handler = new UserCommandHandler();
            
            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnQuitCommand += _eventFunc; }).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count == 0);
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_HelpCommand_HelpHandlerCalled()
        {
            const string command = "help";
            var handler = new UserCommandHandler();
            
            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnHelpCommand += _eventFunc; }).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count == 0);
        }

        [TestMethod]
        public void ProcessInputAsyncCommand_UnknownCommand_UnknownHandlerCalled()
        {
            const string command = "kalvionesljfie";
            var handler = new UserCommandHandler();

            RunProcessInputAsyncCommandTest(command, handler, () => { handler.OnUnknownCommand += _eventFunc; }).Wait();

            Assert.AreEqual(command, _commands[0].Command);
            Assert.IsTrue(_commands[0].Parameters.Count == 0);
        }

        [TestMethod]
        public void ParseUserCommand_CommandOnly_ParsedSuccessfully()
        {
            const string command = "part";
            var rawCommand = string.Format("/{0}", command);
            var handler = new UserCommandHandler();

            var userCommand = handler.ParseUserCommand(rawCommand);

            Assert.AreEqual(command, userCommand.Command);
        }

        [TestMethod]
        public void ParseUserCommand_CommandAndParams_ParsedSuccessfully()
        {
            const string command = "test";
            const string firstParameter = "param1";
            const string secondParameter = "param2";
            var rawCommand = string.Format("/{0} {1} {2}", command, firstParameter, secondParameter);
            var handler = new UserCommandHandler();

            var userCommand = handler.ParseUserCommand(rawCommand);

            Assert.AreEqual(command, userCommand.Command);
            Assert.IsTrue(userCommand.Parameters.Count == 2);
            Assert.IsTrue(userCommand.Parameters.Contains(firstParameter));
            Assert.IsTrue(userCommand.Parameters.Contains(secondParameter));
        }

        private static async Task RunProcessInputAsyncCommandTest(string command, IrcEventHandler handler, Action handlerFunc, params string[] parameters)
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("/{0}", command));

            foreach (var param in parameters)
            {
                builder.Append(string.Format(" {0}", param));
            }

            var rawCommand = builder.ToString();

            handlerFunc();

            await handler.ProcessInputAsync(rawCommand);
        }
    }
}
