module UnitTest.Facts

open System
open System.Collections.Generic
open System.Linq
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Net.Http.Json
open HttpClientMock
open Newtonsoft.Json
open Xunit
open FsUnit.Xunit

[<Fact>]
let should_be_case_insensitive() =

    let builder = MockedHttpClientBuilder();
    builder
        .WhenGet("/Test")
        .Respond(HttpStatusCode.InternalServerError) |> ignore
    use httpClient = builder.Build("http://localhost:1122");
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
    response.StatusCode |> should equal HttpStatusCode.InternalServerError

[<Fact>]
let should_read_as_model_wen_media_type_is_json() =

    let builder = MockedHttpClientBuilder();
    let retrieve = builder
                       .WhenPost("/streams/test")
                       .Respond(HttpStatusCode.OK)
                       .Capture<List<StreamEntity>>()
                       
    use httpClient = builder.Build("http://localhost:1122")
    let content = new StringContent(@"[
              {
                ""eventId"": ""e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"",
                ""id"": 123
              }
            ]")
    
    content.Headers.ContentType = MediaTypeHeaderValue("application/json") |> ignore

    let response = httpClient.PostAsync("http://localhost:1122/streams/test", content).Result;
    
    let _, body = retrieve.Invoke().ToTuple();
    
    response.StatusCode |> should equal HttpStatusCode.OK
    body |> should haveCount 1
    body.[0].Id |> should equal 123L
    body.[0].EventId |> should equal (Guid("e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"))

[<Fact>]
let should_read_string_as_request_body_for_unknown_content_type() =

    let builder = MockedHttpClientBuilder();
    let retrieve = builder
                       .WhenPost("/streams/test")
                       .Respond(HttpStatusCode.OK)
                       .Capture();
    use httpClient = builder.Build("http://localhost:1122")
    
    let result = @"[
              {
                ""eventId"": ""e1fdf1f0-a66d-4f42-95e6-d6588cc22e9b"",
                ""id"": 0
              }
            ]"
    let content = new StringContent(result)
    content.Headers.ContentType = MediaTypeHeaderValue("application/vnd.eventstore.events+json") |> ignore

    let response = httpClient.PostAsync("http://localhost:1122/streams/test", content).Result;
    Assert.Equal(HttpStatusCode.OK, response.StatusCode)    
    Assert.NotNull(retrieve.Invoke().Body<Object>());

[<Fact>]
let should_matches_url_when_it_is_absolute_uri() =

    let builder = MockedHttpClientBuilder()
    builder
        .WhenGet("http://localhost:1122/test")
        .Respond(HttpStatusCode.InternalServerError) |> ignore
    use httpClient = builder.Build("http://localhost:1122")
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);


[<Fact>]
let should_be_able_to_accept_string_content() =
    let builder = MockedHttpClientBuilder()
    let result = " a \"c\" b "
    builder.WhenGet("/test")
        .RespondContent(HttpStatusCode.OK, fun r -> new StringContent(result):>HttpContent ) |> ignore
    use httpClient = builder.Build("http://localhost:1122");
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
    Assert.Equal(result, response.Content.ReadAsStringAsync().Result); // raw string

[<Fact>]
let should_be_able_to_accept_http_content_multiple_times() =
    let builder = MockedHttpClientBuilder()
    let result = " a \"c\" b "
    builder.WhenGet("/test")
        .RespondContent(HttpStatusCode.OK, fun request -> new StringContent(result):> HttpContent) |> ignore
    use httpClient = builder.Build("http://localhost:1122")
    Assert.Equal(result,
        httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result.Content.ReadAsStringAsync().Result); // raw string

    Assert.Equal(result,
        httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result.Content.ReadAsStringAsync().Result); // raw string

[<Fact>]
let should_be_able_to_accept_raw_object() =

    let builder = MockedHttpClientBuilder();
    let result = 56;

    builder.WhenGet("/test").Respond(HttpStatusCode.OK, result) |> ignore
    use httpClient = builder.Build("http://localhost:1122");
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result

    let actual = response.Content.ReadAsStringAsync().Result;
    Assert.Equal(result.ToString(), actual);



[<Fact>]
let should_be_able_to_valid_request() =
    let builder = MockedHttpClientBuilder()
    let result = " a \"c\" b ";
    let requestBody = {| Field = "a"; Field2 = "b" |}

    let retrieve = builder.WhenPost("/test")
                       .RespondContent(HttpStatusCode.OK, fun r -> new StringContent(result):> HttpContent)
                       .Capture(requestBody)

    use httpClient = builder.Build("http://localhost:1122")
    
    let inline tee f v = f v ; v
    
    let content = JsonConvert.SerializeObject(requestBody)
                  |> fun s -> new StringContent(s)
                              |> tee (fun c -> c.Headers.ContentType <- MediaTypeHeaderValue("application/json"))

    let request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:1122/test", Content = content)
    
    let response = httpClient.SendAsync(request).Result;
    let _, body = retrieve.Invoke().ToTuple()
    
    Assert.Equal(result, response.Content.ReadAsStringAsync().Result); // raw string
    Assert.Equal("a", body.Field);
    Assert.Equal("b", body.Field2);


[<Fact>]
let should_be_able_to_accept_custom_header() =

    let builder = MockedHttpClientBuilder()
    let content = "dummy"
    let headerValue = "testHeaderValue"
    builder.WhenGet("/test")
        .RespondContent(HttpStatusCode.OK, fun r -> new StringContent(content):> HttpContent)
        .RespondHeaders({| headerKey = headerValue |}) |> ignore
    use httpClient = builder.Build("http://localhost:1122")
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test")).Result;
    Assert.Equal(content, response.Content.ReadAsStringAsync().Result);
    Assert.Equal(headerValue, response.Headers.GetValues("headerKey").First());

[<Fact>]
let should_be_able_to_match_dollar() =

    let builder = MockedHttpClientBuilder()
    builder.WhenGet("/te$st").Respond(HttpStatusCode.OK) |> ignore
    use httpClient = builder.Build("http://localhost:1122")
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/te$st")).Result
    Assert.Equal(HttpStatusCode.OK, response.StatusCode)


[<Fact>]
let should_be_able_to_retrieve_request() =

    let builder = MockedHttpClientBuilder();
    let capture = builder.WhenGet("/test1").Respond(HttpStatusCode.OK).Capture()
    use httpClient = builder.Build("http://localhost:1122")
    Assert.Null(capture.Invoke())
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test0")).Result
    Assert.Null(capture.Invoke());
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    let response1 = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test1")).Result;
    let requestCapture = capture.Invoke()
    Assert.NotNull(requestCapture);
    Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
    Assert.Equal("GET", requestCapture.Method);
    Assert.Equal("http://localhost:1122/test1", requestCapture.RequestUri.ToString())

    let _ = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/test2")).Result
    let requestCapture2 = capture.Invoke()
    Assert.NotNull(requestCapture2)
    Assert.Equal("http://localhost:1122/test1", requestCapture2.RequestUri.ToString())
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)


[<Fact>]
let should_be_able_to_match_and_retrieve_request_for_anonymous_type() =
    
    let builder = MockedHttpClientBuilder();
    let request = {|name = Unchecked.defaultof<string> |}
    
    let capture = builder.WhenPost("/te$st")
                      .MatchRequest(request, fun r t -> t.name = "John")
                      .Respond(HttpStatusCode.OK)
                      .Capture()
    use httpClient = builder.Build("http://localhost:1122")
    let requestCapture = capture.Invoke()
    Assert.Null(requestCapture);
    let response = httpClient.PostAsJsonAsync("http://localhost:1122/te$st",  {|name = "John"|}).Result;
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    let requestCapture = capture.Invoke()
    Assert.NotNull(requestCapture);


[<Fact>]
let should_be_able_to_match_and_retrieve_request() =
    let builder = MockedHttpClientBuilder();
    let requestRetriever = builder.WhenGet("/te$st")
                               .Respond(HttpStatusCode.OK)
                               .MatchRequest(fun _ _ -> true)
                               .Capture();
    use httpClient = builder.Build("http://localhost:1122");
    let actualRequest = requestRetriever.Invoke()
    Assert.Null(actualRequest);
    let _ = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/te$st")).Result;
    let actualRequest1 = requestRetriever.Invoke();
    Assert.NotNull(actualRequest1);


[<Fact>]
let should_be_able_to_process_string_as_json() =
    let builder = MockedHttpClientBuilder();
    let retrieve = builder.WhenPut("/te$st").Respond(HttpStatusCode.OK, {||}).Capture();
    use httpClient = builder.Build("http://localhost:1122");
    let response = httpClient.PutAsJsonAsync("http://localhost:1122/te$st", "abc").Result;
    Assert.Equal(HttpStatusCode.OK,response.StatusCode);
    Assert.Equal("abc", retrieve.Invoke().Body<Object>().ToString());


[<Fact>]
let should_be_able_to_match_the_last_mocked_request() =
    let builder = MockedHttpClientBuilder();
    let _ = builder.WhenGet("/multi-time-to-mock")
                                    .RespondContent(HttpStatusCode.OK, fun request -> new StringContent("mock uri for first time"):> HttpContent)
                                    .MatchRequest(fun _ _ -> true)
                                    .Capture()
    let secondRequestRetriever = builder.WhenGet("/multi-time-to-mock")
                                     .RespondContent(HttpStatusCode.BadGateway, fun request -> new StringContent("mock uri for second time") :> HttpContent)
                                     .MatchRequest(fun _ _ -> true)
                                     .Capture()
    use httpClient = builder.Build("http://localhost:1122")
    let actualRequest = secondRequestRetriever.Invoke()
    Assert.Null(actualRequest)
    
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:1122/multi-time-to-mock")).Result;
    let actualRequest2 = secondRequestRetriever.Invoke()
    Assert.NotNull(actualRequest2);
    Assert.NotNull(response);
