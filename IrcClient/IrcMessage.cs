using System;
using System.Collections.Generic;
using System.Text;

namespace IrcClient
{
    /// <summary>
    /// A message that can be passed between an IRC server and client.
    /// </summary>
    public class IrcMessage
    {
        /// <summary>
        /// The message prefix, specified by a leading ":" character. Optional.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The message command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// A variable number of message parameters. Optional
        /// </summary>
        public IList<string> Parameters { get; set; }

        /// <summary>
        /// A trailing parameter, specified by a leading ":" character, which
        /// allows spaces. Optional.
        /// </summary>
        public string TrailingParameter { get; set; }

        public IrcMessage()
        {
            Prefix = "";
            Command = "";
            TrailingParameter = "";
            Parameters = new List<string>();
        }

        /// <summary>
        /// Returns a string-representation of the message in valid IRC protocol format.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!String.IsNullOrEmpty(Prefix))
            {
                builder.Append(string.Format(":{0} ", Prefix));
            }

            builder.Append(string.Format("{0} ", Command));

            foreach (var parameter in Parameters)
            {
                builder.Append(string.Format("{0} ", parameter));
            }

            if (!String.IsNullOrEmpty(TrailingParameter))
            {
                builder.Append(string.Format(":{0}", TrailingParameter));
            }

            builder.Append("\r\n");

            var rawMessage = builder.ToString();

            return rawMessage;
        }
    }
}
