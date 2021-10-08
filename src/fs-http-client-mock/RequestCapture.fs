namespace HttpClientMock

open System;
open System.IO;
open System.Net.Http;
open System.Net.Http.Json;
open JetBrains.Annotations;


type RequestCapture (requestUri: Uri ,  method: HttpMethod,  content: HttpContent option) =
    [<PublicAPI>]
    member this.HttpMethod = method
    
    [<PublicAPI>]
    member this.RequestUri = requestUri

    static member Read<'T>(content: HttpContent): 'T =
        match typeof<'T> with
            | t when t = typeof<string> -> (box (content.ReadAsStringAsync().Result) ) :?> 'T
            | t when t = typeof<Stream> -> (box (content.ReadAsStreamAsync().Result)) :?> 'T
            | t when t = typeof<byte[]> -> (box (content.ReadAsByteArrayAsync().Result)) :?> 'T
            | _ -> content.ReadFromJsonAsync<'T>().Result
            
    static member Copy(content: HttpContent ): StreamContent =
        new MemoryStream()
            |> fun m -> content.CopyToAsync(m).Wait(); m
            |> fun m -> m.Position <- 0L; m
            |> fun m -> new StreamContent(m)
            |> fun c -> content.Headers |> Seq.iter (fun h -> c.Headers.Add(h.Key, h.Value));c
            
    [<PublicAPI>]
    member this.Model<'T>(): 'T option =
        match content with
            | None -> None
            | Some c -> Some(RequestCapture.Read<'T>(RequestCapture.Copy(c)))
