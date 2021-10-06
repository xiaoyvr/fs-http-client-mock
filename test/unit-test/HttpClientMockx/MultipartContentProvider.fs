namespace HttpClientMockx

open System.Net.Http
open System.Net.Http.Json
open JetBrains.Annotations

type MultipartContentProvider(content: MultipartContent) = 
    [<PublicAPI>]
    member this.Get<'T>(name: string): 'T =
        let x = content |> Seq.find (fun c -> c.Headers.ContentDisposition.Name = name)
        x.ReadFromJsonAsync<'T>().Result;
        
