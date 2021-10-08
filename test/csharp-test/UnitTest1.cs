using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using HttpClientMock;
using Xunit;


namespace csharp_test
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            var builder = new MockedHttpClientBuilder();
            var capture = builder
                .WhenPost(UrlMatchers.Is("/Some"))
                .MatchRequest( ( _, arg2) => arg2.name.Length > 0, 
                    new {name = default(string)})
                .Respond(HttpStatusCode.OK, new { age = 3 }, 
                    headers: new[] { ("blabla", "aaa"), ("accept", "application/json") },
                    location: new Uri("http://somelocation"))
                .Capture(new {name = default(string)});
            
            using var httpClient = builder.Build("http://localhost:1344");
            

            var response = await httpClient.PostAsJsonAsync("http://localhost:1344/some", new {name = "john"});
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var (requestCapture, o) = capture();
            Assert.Equal(HttpMethod.Post, requestCapture.HttpMethod);
            Assert.Equal("john", o.name);
            Assert.Equal(new Uri("http://somelocation"), response.Headers.Location);
        }
    }
}