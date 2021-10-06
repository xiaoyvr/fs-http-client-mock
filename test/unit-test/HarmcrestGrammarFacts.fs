module UnitTest.HarmcrestGrammarFacts

open System.Net
open System.Net.Http
open System.Net.Http.Json
open HttpClientMock
open Xunit
open FsUnit.Xunit
open type UrlMatchers

let BaseAddress = "http://localhost:1122"
let RequestUri = "http://localhost:1122/staff?employeeId=Staff0001"

[<Theory>]
[<InlineData("/staffs", HttpStatusCode.InternalServerError)>]
[<InlineData("/users", HttpStatusCode.InternalServerError)>]
[<InlineData("/assignees", HttpStatusCode.NotFound)>]
let should_support_is_regex(url: string, expectedStatusCode: HttpStatusCode) =
        
    let serverBuilder = MockedHttpClientBuilder()
    serverBuilder
        .WhenGet(Regex(@"/(staff)|(user)s"))
        .Respond(HttpStatusCode.InternalServerError) |> ignore
        
    use httpClient = serverBuilder.Build(BaseAddress)
    
    let response = httpClient.GetAsync($"{BaseAddress}{url}").Result
    
    response.StatusCode |> should equal expectedStatusCode
    
[<Fact>]
let should_support_head_request() =
    let serverBuilder = MockedHttpClientBuilder();
    serverBuilder
        .When(Regex(@"/staffs"), HttpMethod.Head)
        .Respond(HttpStatusCode.InternalServerError) |> ignore

    use httpClient = serverBuilder.Build(BaseAddress);
    let response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{BaseAddress}/staffs")).Result
    response.StatusCode |> should equal HttpStatusCode.InternalServerError


[<Fact>]
let should_support_is_regex_for_post() =
    let serverBuilder = MockedHttpClientBuilder();
    serverBuilder
        .WhenPost(Regex(@"/staffs"))
        .Respond(HttpStatusCode.InternalServerError) |> ignore

    use httpClient = serverBuilder.Build(BaseAddress);
    let data = {|Name = "Staff"; Email = "emal@staff.com"|}
    let response = httpClient.PostAsync($"{BaseAddress}/staffs", JsonContent.Create(data)).Result
    response.StatusCode |> should equal HttpStatusCode.InternalServerError
    


[<Fact>]
let should_support_it_is_star_wildcard() =

    let serverBuilder = MockedHttpClientBuilder();
    serverBuilder
        .WhenGet(Wildcard(@"/staff*"))
        .Respond(HttpStatusCode.InternalServerError) |> ignore

    use httpClient = serverBuilder.Build(BaseAddress);
    let response = httpClient.GetAsync(RequestUri).Result;
    response.StatusCode |> should equal HttpStatusCode.InternalServerError


[<Fact>]
let should_support_it_is_question_mark_wildcard() =

    let serverBuilder = MockedHttpClientBuilder();
    serverBuilder
        .WhenGet(Wildcard(@"/staffs/?"))
        .Respond(HttpStatusCode.InternalServerError) |> ignore

    use httpClient = serverBuilder.Build(BaseAddress)
    let response = httpClient.GetAsync($"{BaseAddress}/staffs/1").Result
    response.StatusCode |> should equal HttpStatusCode.InternalServerError


[<Fact>]
let should_support_it_is() =
    let serverBuilder = MockedHttpClientBuilder();
    serverBuilder
        .WhenGet(Is("/staff?employeeId=Staff0001"))
        .Respond(HttpStatusCode.InternalServerError) |> ignore

    use httpClient = serverBuilder.Build(BaseAddress);
    let response = httpClient.GetAsync(RequestUri).Result;
    response.StatusCode |> should equal HttpStatusCode.InternalServerError

