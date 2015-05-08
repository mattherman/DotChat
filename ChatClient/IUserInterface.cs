using IrcClient;

namespace ChatClient
{
    public interface IUserInterface
    {
        /// <summary>
        /// Wrapper for the title displayed on the user interface.
        /// </summary>
        string ConsoleTitle { get; set; }

        /// <summary>
        /// Prepare the user interface for use.
        /// </summary>
        void SetupInterface();

        /// <summary>
        /// Write a message to the user interface.
        /// </summary>
        /// <param name="message">The message being written</param>
        void OutputMessage(Message message);

        /// <summary>
        /// Retrieves input from the user interface.
        /// </summary>
        /// <param name="nickname">The nickname of the current user</param>
        /// <returns>Input from the user</returns>
        string GetUserInput(string nickname);
    }
}
