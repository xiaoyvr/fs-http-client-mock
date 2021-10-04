using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using HttpClientMock.Responses;

namespace HttpClientMock
{
    internal class RequestBehavior
    {
        private readonly HttpMethod method;
        private readonly IResponseCreator responseCreator;
        private readonly Uri location;
        private readonly HttpStatusCode statusCode;
        private readonly Func<string, bool> urlMatcher;
        private readonly IRequestProcessor requestProcessor;
        private readonly IDictionary<string, string> headers;
        
        public RequestBehavior(HttpStatusCode statusCode, Func<string, bool> urlMatcher, HttpMethod method, IRequestProcessor requestProcessor, IResponseCreator responseCreator, Uri location, IDictionary<string, string> headers)
        {
            this.method = method;
            this.responseCreator = responseCreator;
            this.location = location;
            this.statusCode = statusCode;
            this.urlMatcher = urlMatcher;
            this.requestProcessor = requestProcessor;
            this.headers = headers;
        }

        public bool Process(HttpRequestMessageWrapper httpRequestMessageWrapper)
        {
            var pathAndQuery = httpRequestMessageWrapper.RequestUri.PathAndQuery;
            var isUriMatch = urlMatcher(pathAndQuery) || urlMatcher(Uri.UnescapeDataString(pathAndQuery));
            var isMethodMatch = httpRequestMessageWrapper.Method.Equals(method.ToString());
            return isUriMatch && isMethodMatch && requestProcessor.Process(httpRequestMessageWrapper);
        }

        public HttpResponseMessage CreateResponseMessage(HttpRequestMessage request)
        {
            HttpResponseMessage httpResponseMessage = responseCreator.CreateResponseFor(request,statusCode);

            if (headers != null && headers.Count > 0)
                headers.ToList().ForEach(header => httpResponseMessage.Headers.Add(header.Key, header.Value));

            httpResponseMessage.Headers.Location = location;
            return httpResponseMessage;
        }
    }
}