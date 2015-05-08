using System.Collections.Generic;

namespace IrcClient
{
	public class UserCommand
	{
		/// <summary>
		/// The user command itself.
		/// </summary>
		public string Command { get; set; }

		/// <summary>
		/// Any parameters associated with the command.
		/// </summary>
		public IList<string> Parameters { get; set; }

		public UserCommand()
		{
			Command = "";
			Parameters = new List<string>();
		}
	}
}
