using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;

namespace HttpClientMock
{
    public class MockedHttpClientBuilder
    {
        private readonly List<RequestBehaviorBuilder> builders = new List<RequestBehaviorBuilder>();

        [PublicAPI]
        public HttpClient Build(string baseAddress)
        {
            var handler = new MockHandler(builders.Select(b => b.Build()).ToArray());
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public RequestBehaviorBuilder WhenGet(string uri)
        {
            return WhenGet(UrlMatchers.Is(uri));
        }

        [PublicAPI]
        public RequestBehaviorBuilder WhenGet(Func<string, bool> urlMatcher)
        {
            return CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Get);
        }

        public RequestBehaviorBuilder WhenPost(string uri)
        {
            return WhenPost(UrlMatchers.Is(uri));
        }

        [PublicAPI]
        public RequestBehaviorBuilder WhenPost(Func<string, bool> urlMatcher)
        {
            return CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Post);
        }

        public RequestBehaviorBuilder WhenPut(string uri)
        {
            return WhenPut(UrlMatchers.Is(uri));
        }

        [PublicAPI]
        public RequestBehaviorBuilder WhenPut(Func<string, bool> urlMatcher)
        {
            return CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Put);
        }

        public RequestBehaviorBuilder WhenDelete(string uri)
        {
            return WhenDelete(UrlMatchers.Is(uri));
        }

        [PublicAPI]
        public RequestBehaviorBuilder WhenDelete(Func<string, bool> urlMatcher)
        {
            return CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Delete);
        }

        [PublicAPI]
        public RequestBehaviorBuilder When(Func<string, bool> urlMatcher, HttpMethod httpMethod)
        {
            return CreateRequestBehaviorBuilder(urlMatcher, httpMethod);
        }

        private RequestBehaviorBuilder CreateRequestBehaviorBuilder(Func<string, bool> urlMatcher, HttpMethod httpMethod)
        {
            var builder = new RequestBehaviorBuilder(urlMatcher, httpMethod);
            builders.Add(builder);
            return builder;
        }
    }
}