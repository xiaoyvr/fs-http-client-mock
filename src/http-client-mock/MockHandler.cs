using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Matcher = System.Func<System.Net.Http.HttpRequestMessage, bool>;
using Responder = System.Func<System.Net.Http.HttpRequestMessage, System.Net.Http.HttpResponseMessage>;

namespace HttpClientMock
{
    internal class MockHandler : DelegatingHandler
    {
        private readonly (Matcher Matcher, Responder Responder)[] behaviors;

        public MockHandler((Matcher Matcher, Responder Responder)[] behaviors)
        {
            this.behaviors = behaviors;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var httpResponseMessage = CreateResponse(request);
            if (request.Method == HttpMethod.Head)
            {
                httpResponseMessage.Content = null;
            }
            return Task.FromResult(httpResponseMessage);
        }
        
        private HttpResponseMessage CreateResponse(HttpRequestMessage request)
        {
            var (_, responder) = behaviors.Reverse().FirstOrDefault(
                behavior => behavior.Matcher(request));
            
            return responder != null ? responder(request) 
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }

    }
}