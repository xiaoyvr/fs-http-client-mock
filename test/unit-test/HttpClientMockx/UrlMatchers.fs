module HttpClientMockx.UrlMatchers

open System;
open System.Text.RegularExpressions;
open JetBrains.Annotations;



[<PublicAPI>]
let Regex(regexPattern: string) =
    fun url ->
        Regex.Match(url, regexPattern, RegexOptions.IgnoreCase).Success;
        
    


[<PublicAPI>]
let Is(url: string) : string -> bool =
    fun pathAndQuery ->
            let lowerStr = url.ToLower()
            pathAndQuery.ToLower() = match Uri.IsWellFormedUriString(lowerStr, UriKind.Absolute) with
                                        | true -> Uri(lowerStr).PathAndQuery
                                        | false ->lowerStr
        
[<PublicAPI>]
let Wildcard( wildCardPattern: string) =
    let regex = new Regex($"^{System.Text.RegularExpressions.Regex.Escape(wildCardPattern)}$"
            .Replace("\\*", ".*")
            .Replace("\\?","."), RegexOptions.IgnoreCase)
    fun s -> regex.Match(s).Success

