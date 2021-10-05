module UnitTest.WildcardMatchFacts

open System.Text.RegularExpressions
open Xunit
open FsUnit.Xunit
open HttpClientMock

[<Theory>]
[<InlineData("pass", true)>]
[<InlineData("pas", false)>]
[<InlineData("passw", true)>]
[<InlineData("password", true)>]
let ``should match star wild card``(s: string, expectedResult: bool) =
    Matchers.Wildcard("pass*").Invoke(s) |> should equal expectedResult 


[<Fact>]
let ``should ignore case when set ignore case``() =
    Matchers.Wildcard("pass*").Invoke("Pass") |> should be True



[<Theory>]
[<InlineData("pass", false)>]
[<InlineData("pas", false)>]
[<InlineData("passw", true)>]
[<InlineData("password", false)>]
let ``should_match_quesiton_mark_as_single_character`` (s: string, expectedResult: bool) =
    Matchers.Wildcard("pass?").Invoke(s) |> should equal expectedResult
            
  