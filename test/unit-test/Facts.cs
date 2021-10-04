using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HttpClientMock;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    public class Facts
    {
        private readonly ITestOutputHelper output;

        public Facts(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void should_be_case_insensitive()
        {
            var builder = new MockedHttpClientBuilder();
            builder
                .WhenGet("/Test")
                .Respond(HttpStatusCode.InternalServerError);
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
        
        [Fact]
        public void should_read_as_model_wen_media_type_is_json()
        {
            var builder = new MockedHttpClientBuilder();
            var retrieve = builder
                .WhenPost("/streams/test")
                .Respond(HttpStatusCode.OK)
                .Capture<List<StreamEntity>>();
            using var httpClient = builder.Build("http://localhost:1122");

            var content = new StringContent(@"[
                      {
                        ""eventId"": ""e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"",
                        ""id"": 123
                      }
                    ]");
            
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = httpClient.PostAsync("http://localhost:1122/streams/test", content).Result;
            
            var (requestCapture, body) = retrieve();
            output.WriteLine(body[0].ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(body);
            Assert.Equal(123, body[0].Id);
            Assert.Equal(new Guid("e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"), body[0].EventId);
            
        }

        [Fact]
        public void should_read_string_as_request_body_for_unknown_content_type()
        {
            var builder = new MockedHttpClientBuilder();
            var retrieve = builder
                .WhenPost("/streams/test")
                .Respond(HttpStatusCode.OK)
                .Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            const string result = @"[
                      {
                        ""eventId"": ""e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"",
                        ""id"": 0
                      }
                    ]";
            var content = new StringContent(result);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.eventstore.events+json");

            var response = httpClient.PostAsync("http://localhost:1122/streams/test", content).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(retrieve().Body<object>());
        }

        [Fact]
        public void should_matches_url_when_it_is_absolute_uri()
        {
            var builder = new MockedHttpClientBuilder();
            builder
                .WhenGet("http://localhost:1122/test")
                .Respond(HttpStatusCode.InternalServerError);
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }


        [Fact]
        public void should_be_able_to_accept_string_content()
        {
            var builder = new MockedHttpClientBuilder();
            const string result = " a \"c\" b ";
            builder.WhenGet("/test")
                .RespondContent(HttpStatusCode.OK, r => new StringContent(result));
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
            Assert.Equal(result, response.Content.ReadAsStringAsync().Result); // raw string
        }

        [Fact]
        public void should_be_able_to_accept_http_content_multiple_times()
        {
            var builder = new MockedHttpClientBuilder();
            const string result = " a \"c\" b ";
            builder.WhenGet("/test")
                .RespondContent(HttpStatusCode.OK, request => new StringContent(result));
            using var httpClient = builder.Build("http://localhost:1122");
            Assert.Equal(result,
                httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result.Content.ReadAsStringAsync().Result); // raw string

            Assert.Equal(result,
                httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result.Content.ReadAsStringAsync().Result); // raw string
        }

        [Fact]
        public void should_be_able_to_accept_raw_object()
        {
            var builder = new MockedHttpClientBuilder();
            var result = 56;

            builder.WhenGet("/test").Respond(HttpStatusCode.OK, result);
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test"))
                .Result;

            var actual = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(result.ToString(), actual);
        }


        [Fact]
        public void should_be_able_to_valid_request()
        {
            var builder = new MockedHttpClientBuilder();
            const string result = " a \"c\" b ";
            
            var requestBody = new { Field = "a", Field2 = "b" };

            var retrieve = builder.WhenPost("/test")
                .RespondContent(HttpStatusCode.OK, r => new StringContent(result))
                .Capture(requestBody);

            using var httpClient = builder.Build("http://localhost:1122");
            
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:1122/test")
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestBody))
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = httpClient.SendAsync(request).Result;
            var actualRequest = retrieve();
            Assert.Equal(result, response.Content.ReadAsStringAsync().Result); // raw string
            Assert.Equal("a", actualRequest.body.Field);
            Assert.Equal("b", actualRequest.body.Field2);
        }

        [Fact]
        public void should_be_able_to_accept_custom_header()
        {
            var builder = new MockedHttpClientBuilder();
            const string content = "dummy";
            const string headerValue = "testHeaderValue";
            builder.WhenGet("/test")
                .RespondContent(HttpStatusCode.OK, r => new StringContent(content))
                .RespondHeaders(new { headerKey = headerValue });
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
            Assert.Equal(content, response.Content.ReadAsStringAsync().Result);
            Assert.Equal(headerValue, response.Headers.GetValues("headerKey").First());
        }
        [Fact]
        public void should_be_able_to_match_dollar()
        {
            var builder = new MockedHttpClientBuilder();
            builder.WhenGet("/te$st").Respond(HttpStatusCode.OK);
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/te$st"))
                .Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void should_be_able_to_retrieve_request()
        {
            var builder = new MockedHttpClientBuilder();
            var capture = builder.WhenGet("/test1").Respond(HttpStatusCode.OK).Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            Assert.Null(capture());
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test0"))
                .Result;
            Assert.Null(capture());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test1"))
                .Result;
            var requestCapture = capture();
            Assert.NotNull(requestCapture);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("GET", requestCapture.Method);
            Assert.Equal("http://localhost:1122/test1", requestCapture.RequestUri.ToString());

            response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test2"))
                .Result;
            var retriever = capture();
            Assert.NotNull(retriever);
            Assert.Equal("http://localhost:1122/test1", retriever.RequestUri.ToString());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public void should_be_able_to_match_and_retrieve_request_for_anonymous_type()
        {
            var builder = new MockedHttpClientBuilder();
            var request = new {name = default(string)};
            var capture = builder.WhenPost("/te$st")
                .MatchRequest(request, (r, t) => t.name == "John")
                .Respond(HttpStatusCode.OK)
                .Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            var requestCapture = capture();
            Assert.Null(requestCapture);
            var response = httpClient.PostAsJsonAsync("http://localhost:1122/te$st", new {name = "John"}).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            requestCapture = capture();
            Assert.NotNull(requestCapture);
        }

        [Fact]
        public void should_be_able_to_match_and_retrieve_request()
        {
            var builder = new MockedHttpClientBuilder();
            var requestRetriever = builder.WhenGet("/te$st")
                .Respond(HttpStatusCode.OK)
                .MatchRequest((r,t) => true)
                .Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            var actualRequest = requestRetriever();
            Assert.Null(actualRequest);
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/te$st")).Result;
            actualRequest = requestRetriever();
            Assert.NotNull(actualRequest);
        }

        [Fact]
        public void should_be_able_to_process_string_as_json()
        {
            var builder = new MockedHttpClientBuilder();
            var retrieve = builder.WhenPut("/te$st").Respond(HttpStatusCode.OK, new {}).Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            var response = httpClient.PutAsJsonAsync("http://localhost:1122/te$st", "abc").Result;
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            Assert.Equal("abc", retrieve().Body<object>().ToString());
        }

        [Fact]
        public void should_be_able_to_match_the_last_mocked_request()
        {
            var builder = new MockedHttpClientBuilder();
            var firstRequestRetriever = builder.WhenGet("/multi-time-to-mock")
                .RespondContent(HttpStatusCode.OK, request => new StringContent("mock uri for first time"))
                .MatchRequest((r,t) => true)
                .Capture();
            var secondRequestRetriever = builder.WhenGet("/multi-time-to-mock")
                .RespondContent(HttpStatusCode.BadGateway, request => new StringContent("mock uri for second time"))
                .MatchRequest((r,t) => true)
                .Capture();
            using var httpClient = builder.Build("http://localhost:1122");
            
                var actualRequest = secondRequestRetriever();
                Assert.Null(actualRequest);
                var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/multi-time-to-mock")).Result;
                actualRequest = secondRequestRetriever();
                Assert.NotNull(actualRequest);
                Assert.NotNull(response);
            
        }
    }
}
