using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using HttpClientMock;
using Xunit;

namespace UnitTest
{
    public class HarmcrestGrammarFacts : TestBase
    {
        private const string BaseAddress = "http://localhost:1122";
        private const string RequestUri = "http://localhost:1122/staff?employeeId=Staff0001";

        [Theory]
        [InlineData("/staffs", HttpStatusCode.InternalServerError)]
        [InlineData("/users", HttpStatusCode.InternalServerError)]
        [InlineData("/assignees", HttpStatusCode.NotFound)]
        public void should_support_is_regex(string url, HttpStatusCode expectedStatusCode)
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenGet(Matchers.Regex(@"/(staff)|(user)s"))
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var response = httpClient.GetAsync($"{BaseAddress}{url}").Result;
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public void should_support_head_request()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .When(Matchers.Regex(@"/staffs"), HttpMethod.Head)
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{BaseAddress}/staffs")).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void should_support_is_regex_for_post()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenPost(Matchers.Regex(@"/staffs"))
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var data = new {Name = "Staff", Email = "emal@staff.com"};
            var response = httpClient.PostAsync($"{BaseAddress}/staffs", JsonContent.Create(data)).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void should_support_it_is_star_wildcard()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenGet(Matchers.Wildcard(@"/staff*"))
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var response = httpClient.GetAsync(RequestUri).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void should_support_it_is_question_mark_wildcard()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenGet(Matchers.Wildcard(@"/staffs/?"))
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var response = httpClient.GetAsync($"{BaseAddress}/staffs/1").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void should_support_it_is()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenGet(Matchers.Is("/staff?employeeId=Staff0001"))
                .Respond(HttpStatusCode.InternalServerError);

            using var httpClient = serverBuilder.Build(BaseAddress);
            var response = httpClient.GetAsync(RequestUri).Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}