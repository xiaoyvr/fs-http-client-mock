using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;

namespace HttpClientMock
{
    public class MultipartContentProvider
    {
        private readonly MultipartContent content;

        public MultipartContentProvider(MultipartContent content)
        {
            this.content = content;
        }

        public T Get<T>(string name)
        {
            var x = this.content.FirstOrDefault(c => c.Headers.ContentDisposition.Name == name);
            return x.ReadFromJsonAsync<T>().Result;
        }
    }
}