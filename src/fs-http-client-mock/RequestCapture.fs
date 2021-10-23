namespace HttpClientMock

open System
open System.IO
open System.Net.Http
open System.Net.Http.Json
open JetBrains.Annotations

module private RequestCapture =
    let Read<'T>(content: HttpContent): 'T =
        match typeof<'T> with
            | t when t = typeof<string> -> (box (content.ReadAsStringAsync().Result) ) :?> 'T
            | t when t = typeof<Stream> -> (box (content.ReadAsStreamAsync().Result)) :?> 'T
            | t when t = typeof<byte[]> -> (box (content.ReadAsByteArrayAsync().Result)) :?> 'T
            | _ -> content.ReadFromJsonAsync<'T>().Result
            
    let Copy(content: HttpContent): StreamContent =
        new MemoryStream()
            |> fun m -> content.CopyToAsync(m).Wait(); m
            |> fun m -> m.Position <- 0L; m
            |> fun m -> new StreamContent(m)
            |> fun c -> content.Headers |> Seq.iter (fun h -> c.Headers.Add(h.Key, h.Value));c

type RequestCapture internal (requestUri: Uri ,  method: HttpMethod,  content: HttpContent option) =
    [<PublicAPI>]
    member this.HttpMethod = method
    
    [<PublicAPI>]
    member this.RequestUri = requestUri
            
    [<PublicAPI>]
    member this.Model<'T>(): 'T option =
        match content with
            | None -> None
            | Some c -> Some(RequestCapture.Read<'T>(RequestCapture.Copy(c)))
type RequestCapturer internal () =
    let mutable captures: RequestCapture seq = Seq.empty
    member internal this.Intake c =
        captures <- Seq.append [c] captures
    member this.Capture() =
        (fun () -> captures |> Seq.tryLast)
            |> FuncUtils.ignoreOption
    member this.Capture<'TR>() =
        fun () ->            
            match captures |> Seq.tryLast with
                | Some r -> Some (r, r.Model<'TR>())
                | None -> None
        |> FuncUtils.ignoreTupleOption
    member this.Capture<'TR>(_schema: 'TR) =
        this.Capture<'TR>()
    member this.CaptureAll() =
        captures |> Array.ofSeq
    member this.CaptureAll<'TR>(?_schema: 'TR) =
        fun () -> captures |> Seq.map(fun c -> (c, c.Model<'TR>())) |> Array.ofSeq
