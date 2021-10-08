namespace HttpClientMock

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Runtime.InteropServices
open JetBrains.Annotations
type RequestBehaviorBuilder internal (urlMatcher: string -> bool, method: HttpMethod) =
    let Obj2Dict( dynObj: Object): Map<string, string> =
        dynObj.GetType().GetProperties()
            |> Seq.map (fun p -> (p.Name, p.GetValue(dynObj, null).ToString() ))
            |> Map.ofSeq
    
    let mutable statusCode: HttpStatusCode = HttpStatusCode.NotFound
    let mutable headerLocation: Uri option = None
    let mutable responseHeaders: Map<string, string> = Map.empty
    let mutable createContent: HttpRequestMessage -> HttpContent = fun _ -> JsonContent.Create("") :> HttpContent
    
    let mutable captures = List.empty 
    
    let mutable matcher: HttpRequestMessage -> bool =
        fun r ->
            match MatchResponse.Match<Object>(r, urlMatcher, method, (fun _ _ -> true)) with
                | Some(c) ->
                    captures <- captures @ [c]
                    true
                | None -> false

    [<PublicAPI>]
    member this.MatchRequest<'TR>(matchFunc: Func<RequestCapture, 'TR, bool>) =
        let mm: RequestCapture -> 'TR option -> bool = fun r tr ->
            match tr with
                | Some ttr -> matchFunc.Invoke(r, ttr)
                | None -> matchFunc.Invoke(r, Unchecked.defaultof<'TR> )

        matcher <- (fun r ->
            match MatchResponse.Match<'TR>(r, urlMatcher, method, mm) with
                | Some(c) ->
                    captures <- captures @ [c]
                    true
                | None -> false
            )
        this

    /// this is only for csharp use since there's no type declaration for anonymous type
    [<PublicAPI>]
    member this.MatchRequest<'TR>(matchFunc: Func<RequestCapture, 'TR, bool>, _: 'TR) =        
        this.MatchRequest<'TR>(matchFunc)
    
    [<PublicAPI>]
    member this.Capture() =
        (fun () -> captures |> Seq.tryLast)
            |> FuncUtils.ignoreOption

    [<PublicAPI>]
    member this.Capture<'TR>() =
        fun () ->            
            match captures |> Seq.tryLast with
                | Some r -> Some (r, r.Model<'TR>())
                | None -> None
        |> FuncUtils.ignoreTupleOption
        
    
    [<PublicAPI>]
    member this.Capture<'TR>(_schema: 'TR) =
        this.Capture<'TR>()
        
    [<PublicAPI>]
    member this.CaptureAll() =
        captures |> Array.ofSeq
    

    [<PublicAPI>]
    member this.CaptureAll<'TR>(?_schema: 'TR) =
        fun () -> captures |> Seq.map(fun c -> (c, c.Model<'TR>())) |> Array.ofSeq;
            
    [<PublicAPI>]
    member this.Respond(status: HttpStatusCode,
                        contentFn: Func<HttpRequestMessage, HttpContent>,
                        [<Optional>]?headers: struct(string*string) seq,
                        [<Optional>]?location: Uri) =        
        statusCode <- status
        createContent <- contentFn.Invoke
        responseHeaders <- (defaultArg headers Seq.empty )
                           |> Seq.map (fun h -> h.ToTuple())
                           |> Map.ofSeq
        headerLocation <- location
        this
        
    [<PublicAPI>]
    member this.Respond(status: HttpStatusCode,
                        [<Optional>]?content: Object,
                        [<Optional>]?headers: struct(string*string) seq,
                        [<Optional>]?location: Uri) =
        let createContent = fun _ -> match content with
                                        | None -> JsonContent.Create("") :> HttpContent
                                        | Some c -> JsonContent.Create(c) :> HttpContent

        this.Respond(status, contentFn = Func<HttpRequestMessage,HttpContent>(createContent),
                     ?headers = headers, ?location = location)
        
    member internal this.Build() : Behavior =
        {
            Matcher = matcher
            Responder = fun r -> MatchResponse.Respond(r, createContent, statusCode, responseHeaders, headerLocation)
        }
