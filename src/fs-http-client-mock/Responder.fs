namespace HttpClientMock

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Runtime.InteropServices

module private Responder =
    let Respond request (createContent: HttpRequestMessage -> HttpContent)
             statusCode (headers: Map<string, string>)
             (location: Uri option): HttpResponseMessage =
    
        new HttpResponseMessage(statusCode, Content = createContent(request))
            |> fun r ->
                headers |> Map.iter (fun k v -> r.Headers.Add(k, v))
                match location with
                    | Some(l) -> r.Headers.Location <- l
                    | None -> ()
                r
                
type Responder internal(requestCapturer: RequestCapturer) =
    let mutable statusCode: HttpStatusCode = HttpStatusCode.NotFound
    let mutable headerLocation: Uri option = None
    let mutable responseHeaders: Map<string, string> = Map.empty
    let mutable createContent: HttpRequestMessage -> HttpContent = fun _ -> JsonContent.Create("") :> HttpContent
    
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
        requestCapturer
        
    member this.Respond(status: HttpStatusCode,
                        [<Optional>]?content: Object,
                        [<Optional>]?headers: struct(string*string) seq,
                        [<Optional>]?location: Uri) =
        let createContent = fun _ -> match content with
                                        | None -> JsonContent.Create("") :> HttpContent
                                        | Some c -> JsonContent.Create(c) :> HttpContent

        this.Respond(status, contentFn = Func<HttpRequestMessage,HttpContent>(createContent),
                     ?headers = headers, ?location = location)
        
    member internal this.Respond r =
        Responder.Respond r createContent statusCode responseHeaders headerLocation
        
