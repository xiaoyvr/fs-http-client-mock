namespace UnitTest

open System.Net
open HttpClientMock
open Xunit
open FsUnit.Xunit

module UriMatchFacts = 

    [<Fact>]
    let ``should match url contains question mark``() =
        let builder = MockedHttpClientBuilder()
        builder.WhenGet("/staff?employeeId=Staff0001")
            .Respond(HttpStatusCode.InternalServerError) |> ignore

        use httpClient = builder.Build("http://localhost:1122")
        async {
            let! response = httpClient.GetAsync("http://localhost:1122/staff?employeeId=Staff0001") |> Async.AwaitTask
            response.StatusCode |> should equal HttpStatusCode.InternalServerError
        } |> Async.RunSynchronously
