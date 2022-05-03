# FsHttpClientMock

[![.NET](https://github.com/xiaoyvr/http-client-mock/actions/workflows/dotnet.yml/badge.svg)](https://github.com/xiaoyvr/http-client-mock/actions/workflows/dotnet.yml)

A really simple http client mock.

## Examples

### Match by simple url

```cs
[Fact]
public async void match_by_simple_url()
{
    var builder = new MockedHttpClientBuilder();
    builder
        .WhenGet("/Test")
        .Respond(HttpStatusCode.OK);s
    using var httpClient = builder.Build("http://localhost:1122");
    var response = await httpClient.GetAsync("http://localhost:1122/test");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

```

### Return some response

```cs
[Fact]
public async void return_some_response()
{
    var builder = new MockedHttpClientBuilder();
    builder
        .WhenGet("/Test")
        .Respond(HttpStatusCode.OK, new People { Name = "John Doe"});

    using var httpClient = builder.Build("http://localhost:1122");
    var response = await httpClient.GetAsync("http://localhost:1122/test");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var people = await response.Content.ReadFromJsonAsync<People>();
    Assert.Equal("John Doe", people?.Name);
}
```

### Capture request being sent

```cs
[Fact]
public async void capture_request_being_sent()
{
    var builder = new MockedHttpClientBuilder();
    var capture = builder.WhenGet("/test").Respond(HttpStatusCode.OK).Capture();
    using var httpClient = builder.Build("http://localhost:1122");
    // no request being sent yet
    Assert.Null(capture());

    // when send the request
    var response = await httpClient.GetAsync("http://localhost:1122/test");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // should get the request by retriever
    var requestCapture = capture();
    Assert.NotNull(requestCapture);
    Assert.Equal(HttpMethod.Get, requestCapture.HttpMethod);
    Assert.Equal("http://localhost:1122/test", requestCapture.RequestUri.ToString());
}
```

### Hamcrest Style Matchers

* **Matchers.Regex**

```cs
[Fact]
public async void hamcrest_style_matchers()
{
    var builder = new MockedHttpClientBuilder();
    builder
        .WhenGet(UrlMatchers.Regex(@"/(staff)|(user)s"))
        .Respond(HttpStatusCode.InternalServerError);

    using var httpClient = builder.Build("http://localhost:1122");
    var response = await httpClient.GetAsync("http://localhost:1122/users");

    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);        
}
```

* **Matchers.WildCard**

```cs
builder
    .WhenGet(Matchers.Wildcard(@"/staffs/?"))
    .Respond(HttpStatusCode.Unauthorized);
```

* **Matchers.Is**

```cs
builder
     .WhenGet(Matchers.Is("/staffs"))
     .Respond(HttpStatusCode.OK);
```
