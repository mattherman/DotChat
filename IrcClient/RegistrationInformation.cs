namespace IrcClient
{
    /// <summary>
    /// Information used to register a new client with an IRC server.
    /// </summary>
    public class RegistrationInformation
    {
        /// <summary>
        /// A password for the server. Optional.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The user's NICKname by which they will be identified
        /// on the server.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The user's username on the server. Don't really know
        /// what this is actually used for. Optional.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The user's real name on the server. Don't really know
        /// what this is actually used for. Optional.
        /// </summary>
        public string RealName { get; set; }
    }
}
