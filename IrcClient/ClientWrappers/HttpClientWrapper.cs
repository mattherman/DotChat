using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace IrcClient.ClientWrappers
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        readonly HttpClient _client;

        public HttpClientWrapper()
        {
            _client = new HttpClient();
        }

        public Uri BaseAddress
        {
            get { return _client.BaseAddress; }
            set { _client.BaseAddress = value; }
        }

        public async Task<Stream> GetStreamAsync(string request)
        {
            return await _client.GetStreamAsync(request);
        }
    }
}
