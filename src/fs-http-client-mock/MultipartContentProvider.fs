namespace HttpClientMock

open System.Net.Http
open System.Net.Http.Json
open JetBrains.Annotations

type MultipartContentProvider(content: MultipartContent) = 
    [<PublicAPI>]
    
    member this.Get<'T>(name: string): 'T =
        content
            |> Seq.find (fun c -> c.Headers.ContentDisposition.Name = name)
            |> fun x -> x.ReadFromJsonAsync<'T>().Result;
        