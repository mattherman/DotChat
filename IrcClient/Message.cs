
namespace IrcClient
{
    /// <summary>
    /// A user-friendly version of a message received by the client. Will be passed
    /// back to a consumer of the library.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The user who sent the message.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The text of the message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public MessageType Type { get; set; }
    }
}
