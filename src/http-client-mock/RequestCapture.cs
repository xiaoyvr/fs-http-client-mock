using System;
 using System.IO;
 using System.Net.Http;
 using System.Net.Http.Json;
using JetBrains.Annotations;

namespace HttpClientMock
{
    public class RequestCapture
    {
        private readonly HttpContent content;
        public RequestCapture(Uri requestUri, HttpMethod method, HttpContent content)
        {
            Method = method;
            this.content = content;
            RequestUri = requestUri;
        }
        
        [PublicAPI]
        public HttpMethod Method { get; }
        [PublicAPI]
        public Uri RequestUri { get; }

        public T Model<T>(T schema = default)
        {
            return content is EmptyContent ? 
                default : Read<T>(Copy(content));
        }
        
        private static T Read<T>(HttpContent content)
        {
            if (typeof(T) == typeof(string))
            {
                return (T) (object) content.ReadAsStringAsync().Result;
            }
            if (typeof(T) == typeof(Stream))
            {
                return (T) (object) content.ReadAsStreamAsync().Result;
            }
            if (typeof(T) == typeof(byte[]))
            {
                return (T) (object) content.ReadAsByteArrayAsync().Result;
            }
            return content.ReadFromJsonAsync<T>().Result;
        }

        private static StreamContent Copy(HttpContent content)
        {
            var memoryStream = new MemoryStream();
            content.CopyToAsync(memoryStream).Wait();
            memoryStream.Position = 0;
            var streamContent = new StreamContent(memoryStream);
            foreach (var (key, value) in content.Headers)
            {
                streamContent.Headers.Add(key, value);    
            }
            return streamContent;
        }
    }
}