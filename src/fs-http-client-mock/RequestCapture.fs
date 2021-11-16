namespace HttpClientMock

open System
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.Runtime.InteropServices

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
    member this.HttpMethod = method
    member this.RequestUri = requestUri
    member internal this.ToModel<'T>(): 'T option =
        content |> Option.map
                       (fun c -> RequestCapture.Read<'T>(RequestCapture.Copy(c)))
    member this.Model<'T> ([<Optional>]schema:'T): 'T  =
        match this.ToModel() with
            | None -> Unchecked.defaultof<'T>
            | Some v -> v
                       
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
                | Some r -> Some (r, r.ToModel<'TR>())
                | None -> None
        |> FuncUtils.ignoreTupleOption
    member this.Capture<'TR>(_schema: 'TR) =
        this.Capture<'TR>()
    member this.CaptureAll() =
        captures |> Array.ofSeq
    member this.CaptureAll<'TR>(?_schema: 'TR) =
        fun () -> captures |> Seq.map(fun c -> (c, c.ToModel<'TR>())) |> Array.ofSeq
