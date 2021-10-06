namespace HttpClientMockx

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open JetBrains.Annotations

type RequestBehaviorBuilder(urlMatcher: string -> bool, method: HttpMethod) =     
    
    let Obj2Dict( dynObj: Object): Map<string, string> =
        dynObj.GetType().GetProperties()
            |> Seq.map (fun p -> (p.Name, p.GetValue(dynObj, null).ToString() ))
            |> Map.ofSeq
            
    
    let mutable statusCode: HttpStatusCode = HttpStatusCode.NotFound
    let mutable headerLocation: Uri option = None
    let mutable responseHeaders: Map<string, string> = Map.empty
    let mutable createContent: HttpRequestMessage -> HttpContent = fun _ -> JsonContent.Create("") :> HttpContent
    let mutable captures: List<RequestCapture> = List.empty
    let mutable matcher = fun r ->
        let ma, newCaptures = MatchResponse.Match<Object>(r, urlMatcher, method, (fun _ _ -> true), captures)
        captures <- newCaptures
        ma
    
    [<PublicAPI>]
    member this.MatchRequest<'TR>(?matchFunc: HttpClientMockx.RequestCapture -> 'TR option -> bool) =
        let m = defaultArg matchFunc (fun _ _ -> true)
        matcher <- (fun r ->
            let ma, newCaptures = MatchResponse.Match<'TR>(r, urlMatcher, method, m, captures)
            captures <- newCaptures
            ma)
        this

    [<PublicAPI>]
    member this.MatchRequest<'TR>(?_schema: 'TR, ?matchFunc: RequestCapture -> 'TR -> bool) =
        this.MatchRequest<'TR>( ?matchFunc = matchFunc )
        
    [<PublicAPI>]
    member this.MatchRequest(?matchFunc: RequestCapture -> Object -> bool) =
        this.MatchRequest<Object>(?matchFunc =matchFunc)
    
    [<PublicAPI>]
    member this.Capture() =
        fun () -> (captures |> Seq.tryLast )

    [<PublicAPI>]
    member this.Capture<'TR>(?_schema: 'TR) =
        fun () ->
            let requestCapture = captures |> Seq.tryLast
            match requestCapture with
                | None -> (None, None)
                | Some(r) -> (Some(r), r.Model<'TR>())
        
    [<PublicAPI>]
    member this.CaptureAll() =
        captures |> Array.ofSeq
    

    [<PublicAPI>]
    member this.CaptureAll<'TR>(?_schema: 'TR) =
        fun () -> captures |> Seq.map(fun c -> (c, c.Model<'TR>())) |> Array.ofSeq;
            
    [<PublicAPI>]
    member this.RespondContent(httpStatusCode: HttpStatusCode, contentFn: HttpRequestMessage -> HttpContent, ?location: Uri) =    
        statusCode <- httpStatusCode
        createContent <- contentFn
        headerLocation <- location
        this
        
    [<PublicAPI>]
    member this.Respond( httpStatusCode: HttpStatusCode, ?content: Object, ?location: Uri) =
        let create = (fun _ -> JsonContent.Create(defaultArg content (box "")) :> HttpContent)
        this.RespondContent(httpStatusCode, create, ?location = location)        
    
    [<PublicAPI>]
    member this.RespondHeaders( headers: Object) =
        responseHeaders <- Obj2Dict(headers);
        this
        
    member this.Build() : Behavior =
        {
            Matcher = matcher;
            Responder = fun r -> MatchResponse.Respond(r, createContent, statusCode, responseHeaders, headerLocation)
        }
        
    

        
    

