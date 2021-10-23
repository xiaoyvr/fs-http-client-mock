module HttpClientMock.UrlMatchers

open System;
open System.Text.RegularExpressions;

let Regex(regexPattern: string) =
    let f = fun url ->
        Regex.Match(url, regexPattern, RegexOptions.IgnoreCase).Success
    Func<string, bool>(f)

let Is (url: string) =
    let f = fun (pathAndQuery: string) ->
            let lowerStr = url.ToLower()
            pathAndQuery.ToLower() = match Uri.IsWellFormedUriString(lowerStr, UriKind.Absolute) with
                                        | true -> Uri(lowerStr).PathAndQuery
                                        | false ->lowerStr
    Func<string, bool>(f)
        
let Wildcard( wildCardPattern: string) =
    let regex = new Regex($"^{System.Text.RegularExpressions.Regex.Escape(wildCardPattern)}$"
            .Replace("\\*", ".*")
            .Replace("\\?","."), RegexOptions.IgnoreCase)
    Func<string, bool>(fun s -> regex.Match(s).Success)

