using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace HttpClientMock.Responses
{
    public class ObjectResponseCreator : IResponseCreator
    {
        private readonly object content;

        public ObjectResponseCreator(object content)
        {
            this.content = content;
        }

        public HttpResponseMessage CreateResponseFor(HttpRequestMessage request, HttpStatusCode statusCode)
        {
            var httpResponseMessage = new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(content)  
            };
            return httpResponseMessage;
        }
    }
}