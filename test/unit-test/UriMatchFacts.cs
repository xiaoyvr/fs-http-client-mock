using System.Net;
using HttpClientMock;
using Xunit;

namespace UnitTest
{
    public class UriMatchFacts
    {
        [Fact]
        public void should_match_url_contains_question_mark()
        {
            var serverBuilder = new MockedHttpClientBuilder();
            serverBuilder
                .WhenGet("/staff?employeeId=Staff0001")
                .Respond(HttpStatusCode.InternalServerError);

            const string baseAddress = "http://localhost:1122";
            using (var httpClient = serverBuilder.Build(baseAddress))
            {
                const string requestUri = "http://localhost:1122/staff?employeeId=Staff0001";
                var response = httpClient.GetAsync(requestUri).Result;
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }
    }
}