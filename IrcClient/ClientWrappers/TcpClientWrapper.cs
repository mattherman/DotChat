using System.IO;
using System.Net.Sockets;

namespace IrcClient.ClientWrappers
{
	public class TcpClientWrapper : ITcpClientWrapper
	{
		private readonly TcpClient _client;
		public TcpClientWrapper()
		{
			_client = new TcpClient();
		}

		public void Connect(string hostname, int port)
		{
			_client.Connect(hostname, port);
		}

		public Stream GetStream()
		{
			return _client.GetStream();
		}
	}
}
