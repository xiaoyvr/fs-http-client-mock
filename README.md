HttpClientMock
==============

A really simple http mock using self host service.

### Match by simple url

```cs
var builder = new MockedHttpClientBuilder();
builder
    .WhenGet("/Test")
    .Respond(HttpStatusCode.OK);
using var httpClient = builder.Build("http://localhost:1122");
var response = await httpClient.GetAsync("http://localhost:1122/test");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

```

### Return some response
```cs
var builder = new MockedHttpClientBuilder();
builder
    .WhenGet("/Test")
    .Respond(HttpStatusCode.OK, new People { Name = "John Doe"});
    
using var httpClient = builder.Build("http://localhost:1122");
var response = await httpClient.GetAsync("http://localhost:1122/test");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
var people = await response.ReadAsAsync<People>();
Assert.Equal("John Doe", people.Name);

```

### Capture request being sent

```cs
var builder = new MockedHttpClientBuilder();
var capture = builder.WhenGet("/test").Respond(HttpStatusCode.OK).Capture();
using var httpClient = builder.Build("http://localhost:1122");
// no request being sent yet
Assert.Null(capture());

// when send the request
var response = await httpClient.GetAsync("http://localhost:1122/test1");

// should get the request by retriever
var requestCapture = capture();
Assert.NotNull(requestCapture);
Assert.Equal(HttpStatusCode.OK, response.StatusCode);                    
Assert.Equal("GET", requestCapture.Method);
Assert.Equal("http://localhost:1122/test", requestCapture.RequestUri.ToString());
```

### Hamcrest Style Matchers

* **Matchers.Regex**

```cs
 var serverBuilder = new MockedHttpClientBuilder();
 builder
     .WhenGet(Matchers.Regex(@"/(staff)|(user)s"))
     .Respond(HttpStatusCode.InternalServerError);

using var httpClient = builder.Build("http://localhost:1122");
var response = await httpClient.GetAsync("http://localhost:1122/users");

Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
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


### Other Matchers

```cs	
builder
    .When(Matchers.Wildcard(@"/staffs/?"), HttpMethod.Patch)
    .Respond(HttpStatusCode.Unauthorized);
```
