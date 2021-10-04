using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMock
{
    internal class EmptyContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0L;
            return true;
        }
    }
}