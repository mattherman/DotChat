using System;
using System.IO;
using System.Threading.Tasks;

namespace IrcClient.ClientWrappers
{
    public interface IHttpClientWrapper
    {
        Uri BaseAddress { get; set; }

        Task<Stream> GetStreamAsync(string request);
    }
}
