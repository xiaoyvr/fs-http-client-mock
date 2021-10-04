using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMock
{
    internal class MockHandler : DelegatingHandler
    {
        private readonly RequestBehaviors requestBehaviors;

        public MockHandler(RequestBehaviors requestBehaviors)
        {
            this.requestBehaviors = requestBehaviors;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var httpResponseMessage = requestBehaviors.CreateResponse(request);
            if (request.Method == HttpMethod.Head)
            {
                httpResponseMessage.Content = null;
            }
            return Task.FromResult(httpResponseMessage);
        }
    }
}