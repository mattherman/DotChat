using IrcClient;
using IrcClient.ClientWrappers;
using Microsoft.Practices.Unity;

namespace ChatClient
{
	class Program
	{
	    private static IUnityContainer _container;

		public static void Main()
		{
            RegisterDependencies();
		    var app = _container.Resolve<IrcChatApplication>();
            app.Start().Wait();
		}

	    private static void RegisterDependencies()
	    {
            _container = new UnityContainer();
	        _container.RegisterType<IrcChatApplication, IrcChatApplication>();
            _container.RegisterType<IClient, Client>();
	        _container.RegisterType<IUserInterface, ConsoleUserInterface>();
	        _container.RegisterType<IHttpClientWrapper, HttpClientWrapper>();
		    _container.RegisterType<ITcpClientWrapper, TcpClientWrapper>();
	    }
	}
}
