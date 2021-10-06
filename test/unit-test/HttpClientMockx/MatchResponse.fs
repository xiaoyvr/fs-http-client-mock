module HttpClientMockx.MatchResponse

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers

let Respond( request: HttpRequestMessage, createContent: HttpRequestMessage -> HttpContent,
             statusCode: HttpStatusCode, headers: Map<string, string>,
             location: Uri option): HttpResponseMessage =
    
    new HttpResponseMessage(statusCode, Content = createContent(request))
        |> fun r ->
            headers |> Map.iter (fun k v -> r.Headers.Add(k, v))
            match location with
                | Some(l) -> r.Headers.Location <- l
                | None -> ()
            r 

let Match<'TR>(request: HttpRequestMessage, urlMatcher: string -> bool , method: HttpMethod,
               matchFunc: RequestCapture -> 'TR option -> bool, captures: RequestCapture list  ) =

    match request with
        | r when r.Method <> method -> false, captures
        | r when not (r.RequestUri.PathAndQuery |> urlMatcher)
                 && not ( r.RequestUri.PathAndQuery |> Uri.UnescapeDataString |> urlMatcher) ->
            false, captures
        | r ->
            let content = match r.Content with
                            | null -> None
                            | _ -> Some(r.Content)
                            
            let capture = RequestCapture(r.RequestUri, r.Method, content)
            let model = capture.Model<'TR>()
            match matchFunc capture model with
                | true -> true, captures @ [capture]
                | false -> false, captures            
