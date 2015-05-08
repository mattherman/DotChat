using System;
using System.Configuration;
using System.Globalization;
using IrcClient;

namespace ChatClient
{
	public class ConsoleUserInterface : IUserInterface
	{
		public string ConsoleTitle 
		{ 
			get { return Console.Title; }
			set { Console.Title = value; } 
		}

		private const string InputIdentifier = " > ";
		private const int NumInputLines = 1;

        private readonly bool _morseCodeModeActive;

	    public ConsoleUserInterface()
	    {
            bool morseCodeModeActive;
            var success = bool.TryParse(ConfigurationManager.AppSettings["MorseCodeModeActive"], out morseCodeModeActive);
            _morseCodeModeActive = success && morseCodeModeActive;
	    }

		public void SetupInterface()
		{
			Console.Clear();
			ConsoleTitle = string.Empty;
			Console.SetCursorPosition(Console.WindowLeft, Console.WindowHeight);
			Console.Write(InputIdentifier);
		}

		public void OutputMessage(Message message)
		{
			var messageText = GetFormattedMessage(message.Text, message.User, message.Type);

			var numLines = 1 + (messageText.Length / Console.WindowWidth);

            var targetTop = (Console.WindowTop - numLines + NumInputLines);
		    targetTop = targetTop >= 1 ? targetTop : 1;

			Console.MoveBufferArea(Console.WindowLeft, Console.WindowTop + numLines, Console.WindowWidth, Console.WindowHeight - numLines - NumInputLines,
									Console.WindowLeft, targetTop);

			var cursorX = Console.CursorLeft;
			var cursorY = Console.CursorTop;

			Console.SetCursorPosition(Console.WindowLeft, cursorY - numLines);
			Console.WriteLine(messageText);
			Console.SetCursorPosition(cursorX, cursorY);
		}

		public string GetUserInput(string nickname)
		{
			var input = Console.ReadLine();
			
			ModifyOutput(input, nickname);
			
			Console.Write(InputIdentifier);

			return input;
		}

		/// <summary>
		/// Modifies the last line(s) of data output to the console and adds
		/// a timestamp and username to them. Necessary in order to make user input
		/// appear the same as all other messages.
		/// </summary>
		/// <param name="input">The user input that is being modified</param>
		/// <param name="nickname">The nickname of the user that wrote the input</param>
		private static void ModifyOutput(string input, string nickname)
		{
			var cursorX = Console.CursorLeft;
			var cursorY = Console.CursorTop;

			var modifiedOutput = GetFormattedMessage(input, nickname, MessageType.User);
			var numLines = 1 + (modifiedOutput.Length / Console.WindowWidth);

			for (var i = 1; i <= numLines; i++)
			{
				Console.SetCursorPosition(Console.WindowLeft, cursorY - i);
				Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
			}
			Console.Write(modifiedOutput);

			Console.SetCursorPosition(cursorX, cursorY);
		}

		/// <summary>
		/// Formats the message for output with a current timestamp, the user's
		/// nickname, and the message.
		/// </summary>
		/// <param name="message">The message being output</param>
		/// <param name="name">The name of the user/server the message belongs to</param>
		/// <param name="messageType">The type of the message</param>
		/// <returns>A formatted string for output to the console</returns>
		internal static string GetFormattedMessage(string message, string name, MessageType messageType)
		{
			var timestamp = DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("hr-HR"));

		    var formattedMessage = "";
		    switch (messageType)
		    {
		        case MessageType.User:
                    formattedMessage = string.Format("[{0}] <{1}> {2}", timestamp, name, message);
		            break;
                case MessageType.Server:
		            formattedMessage = string.Format("[{0}] == {1}", timestamp, message);
		            break;
                case MessageType.Private:
		            formattedMessage = string.Format("[{0}] *{1}* {2}", timestamp, name, message);
		            break;
		    }
		    return formattedMessage;
		}
	}
}
