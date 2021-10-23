namespace HttpClientMock

open System
open System.Net.Http

type MockedHttpClientBuilder() =
    
    let mutable builders: MatcherResponder list = List.empty
    
    let CreateRequestMatcherResponder(urlMatcher: Func<string, bool>, httpMethod: HttpMethod) =
        
        let matcher = MatcherResponder.MatchUrl (FuncConvert.FromFunc urlMatcher)
                      >> Option.bind (MatcherResponder.MatchMethod httpMethod)
        let b = MatcherResponder(matcher)
        builders <- builders @ [b]
        b
            
    member this.MatchRequest<'TR>(matchFunc: Func<RequestCapture, 'TR, bool>) =
        let b = MatcherResponder(Some)
        builders <- builders @ [b]
        b.MatchRequest(matchFunc)
                
    member this.Build(baseAddress: string) =
        new MockHandler(builders |> Seq.map(fun b -> b.Build() ) |> Array.ofSeq)
            |> fun h -> new HttpClient(h, BaseAddress = Uri(baseAddress))

    member this.WhenGet(urlMatcher: Func<string, bool>) =
        CreateRequestMatcherResponder(urlMatcher, HttpMethod.Get)

    member this.WhenGet(url: string) =
        this.WhenGet(UrlMatchers.Is(url));
        
        
    member this.WhenPost(urlMatcher: Func<string, bool>) =
        CreateRequestMatcherResponder(urlMatcher, HttpMethod.Post)
    
    member this.WhenPost(uri: string) =
        this.WhenPost(UrlMatchers.Is(uri))

    member this.WhenPut(urlMatcher: Func<string, bool>) =
        CreateRequestMatcherResponder(urlMatcher, HttpMethod.Put)
        
    member this.WhenPut(url: string) =
        this.WhenPut(UrlMatchers.Is(url))

    member this.WhenDelete(urlMatcher: Func<string, bool>) =
        CreateRequestMatcherResponder(urlMatcher, HttpMethod.Delete)
        
    member this.WhenDelete(url: string) =
        this.WhenDelete(UrlMatchers.Is(url))
    

    /// this is only for csharp use since there's no type declaration for anonymous type
    member this.MatchRequest<'TR>(matchFunc: Func<RequestCapture, 'TR, bool>, _: 'TR) =        
        this.MatchRequest<'TR>(matchFunc)
    
    member this.When(urlMatcher: Func<string, bool>,  httpMethod: HttpMethod) =
        CreateRequestMatcherResponder(urlMatcher, httpMethod);
    
