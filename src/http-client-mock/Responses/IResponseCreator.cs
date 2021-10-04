using System.Net;
using System.Net.Http;

namespace HttpClientMock.Responses
{
    public interface IResponseCreator
    {
        HttpResponseMessage CreateResponseFor(HttpRequestMessage request, HttpStatusCode statusCode);
    }
}