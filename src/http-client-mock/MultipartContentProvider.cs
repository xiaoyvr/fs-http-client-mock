using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using JetBrains.Annotations;

namespace HttpClientMock
{
    public class MultipartContentProvider
    {
        private readonly MultipartContent content;

        public MultipartContentProvider(MultipartContent content)
        {
            this.content = content;
        }

        [PublicAPI]
        public T Get<T>(string name)
        {
            var x = content.First(c => c.Headers.ContentDisposition.Name == name);
            return x.ReadFromJsonAsync<T>().Result;
        }
    }
}