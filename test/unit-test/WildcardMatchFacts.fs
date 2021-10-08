module UnitTest.WildcardMatchFacts

open Xunit
open FsUnit.Xunit
open HttpClientMock.UrlMatchers

[<Theory>]
[<InlineData("pass", true)>]
[<InlineData("pas", false)>]
[<InlineData("passw", true)>]
[<InlineData("password", true)>]
let ``should match star wild card``(s: string, expectedResult: bool) =
    Wildcard("pass*").Invoke(s) |> should equal expectedResult 


[<Fact>]
let ``should ignore case when set ignore case``() =
    Wildcard("pass*").Invoke("Pass") |> should be True



[<Theory>]
[<InlineData("pass", false)>]
[<InlineData("pas", false)>]
[<InlineData("passw", true)>]
[<InlineData("password", false)>]
let ``should match question mark as single character`` (s: string, expectedResult: bool) =
    Wildcard("pass?").Invoke(s) |> should equal expectedResult
            
  