namespace IrcClient
{
    /// <summary>
    /// Information about the IRC server being connected to.
    /// </summary>
    public class ServerInformation
    {
        /// <summary>
        /// The hostname of the IRC server.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The number of the port to connect on.
        /// </summary>
        public int Port { get; set; }
    }
}
