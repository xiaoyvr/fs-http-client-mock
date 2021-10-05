using System.Net;
using System.Net.Http;

namespace HttpClientMock.Responses
{
    internal interface IResponseCreator
    {
        HttpResponseMessage CreateResponseFor(HttpRequestMessage request, HttpStatusCode statusCode);
    }
}