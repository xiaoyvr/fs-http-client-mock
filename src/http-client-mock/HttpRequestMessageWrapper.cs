using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Xml;

namespace HttpClientMock
{
    internal class HttpRequestMessageWrapper
    {
        private readonly HttpRequestMessage request;
        private RequestCapture requestCapture;
        public string Method { get; }

        public HttpRequestMessageWrapper(HttpRequestMessage request)
        {
            this.request = request;
            RequestUri = request.RequestUri;
            Method = request.Method.Method;            
        }

        public RequestCapture GetRequestCapture()
        {
            return requestCapture ??= new RequestCapture
            {
                RequestUri = request.RequestUri,
                Method = request.Method.ToString(),
                Content = request.Content == null ? (HttpContent)new EmptyContent() : Copy(request.Content),
            };
        }

        private static StreamContent Copy(HttpContent content)
        {
            var memoryStream = new MemoryStream();
            content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var streamContent = new StreamContent(memoryStream);
            foreach (var (key, value) in content.Headers)
            {
                streamContent.Headers.Add(key, value);    
            }
            return streamContent;
        }

        public Uri RequestUri { get; }
    }
}