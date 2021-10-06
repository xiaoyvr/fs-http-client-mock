namespace HttpClientMockx

open System
open System.Net.Http
open JetBrains.Annotations

type MockedHttpClientBuilder() =
    
    let mutable builders: RequestBehaviorBuilder list = List.empty
    
    let CreateRequestBehaviorBuilder( urlMatcher: string -> bool, httpMethod: HttpMethod) =
        let builder = RequestBehaviorBuilder(urlMatcher, httpMethod);
        builders <- builders @ [builder];
        builder
    
    [<PublicAPI>]
    member this.Build(baseAddress: string) =
        new MockHandler(builders |> Seq.map(fun b -> b.Build() ) |> Array.ofSeq)
            |> fun h -> new HttpClient(h, BaseAddress = Uri(baseAddress))

    [<PublicAPI>]
    member this.WhenGet(urlMatcher: string -> bool) =
        CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Get);

    [<PublicAPI>]            
    member this.WhenGet(uri: string) =
        this.WhenGet(UrlMatchers.Is(uri));
        
        
    [<PublicAPI>]
    member this.WhenPost(urlMatcher: string -> bool) =
        CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Post)
    
    [<PublicAPI>]
    member this.WhenPost(uri: string) =
        this.WhenPost(UrlMatchers.Is(uri))

    [<PublicAPI>]
    member this.WhenPut(urlMatcher: string -> bool) =
        CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Put)
        
    [<PublicAPI>]
    member this.WhenPut(url: string) =
        this.WhenPut(UrlMatchers.Is(url))

    [<PublicAPI>]
    member this.WhenDelete(urlMatcher: string -> bool) =
        CreateRequestBehaviorBuilder(urlMatcher, HttpMethod.Delete)
        
    [<PublicAPI>]
    member this.WhenDelete(url: string) =
        this.WhenDelete(UrlMatchers.Is(url))
    
    [<PublicAPI>]
    member this.When(urlMatcher: string -> bool,  httpMethod: HttpMethod) =
        CreateRequestBehaviorBuilder(urlMatcher, httpMethod);
    