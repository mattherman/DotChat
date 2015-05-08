using System.IO;

namespace IrcClient.ClientWrappers
{
	public interface ITcpClientWrapper
	{
		void Connect(string hostname, int port);

		Stream GetStream();
	}
}
