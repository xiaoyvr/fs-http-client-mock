module Tests

open Microsoft.AspNetCore.Hosting
open Xunit

[<Fact>]
let ``My test`` () =
    let builder = new WebHostBuilder()
    let testServer = new TestServer(builder);
    Assert.True(true)
