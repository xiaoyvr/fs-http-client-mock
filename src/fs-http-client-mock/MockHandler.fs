namespace HttpClientMock

open System.Net
open System.Net.Http
open System.Threading.Tasks


type Behavior = {Matcher: HttpRequestMessage -> bool; Responder: HttpRequestMessage -> HttpResponseMessage}

type MockHandler(behaviors: Behavior[]) =
    inherit DelegatingHandler()
    
    override this.SendAsync(request: HttpRequestMessage, _: System.Threading.CancellationToken) =
        match behaviors |> Seq.tryFindBack(fun b -> b.Matcher(request)) with
                | None -> new HttpResponseMessage(HttpStatusCode.NotFound)
                | Some(b) -> b.Responder(request)
            |> fun r -> if request.Method = HttpMethod.Head then r.Content <- null
                        r
            |> Task.FromResult
