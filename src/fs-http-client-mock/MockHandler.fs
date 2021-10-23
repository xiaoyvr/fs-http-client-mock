namespace HttpClientMock

open System.Net
open System.Net.Http
open System.Threading.Tasks


type internal Behavior = {
                           Match: HttpRequestMessage -> HttpRequestMessage option
                           Respond: HttpRequestMessage -> HttpResponseMessage
                           Capture: RequestCapture -> unit 
                           }

type internal MockHandler(behaviors: Behavior[]) =
    inherit DelegatingHandler()
    
    override this.SendAsync(request: HttpRequestMessage, _: System.Threading.CancellationToken) =
        match behaviors |> FuncUtils.tryFindMapBack(fun b -> b.Match(request)) with
                | None -> new HttpResponseMessage(HttpStatusCode.NotFound)
                | Some(r, b) ->
                    b.Capture(RequestCapture(r.RequestUri, r.Method, Option.ofObj r.Content))
                    b.Respond(r)
            |> fun r -> if request.Method = HttpMethod.Head then r.Content <- null
                        r
            |> Task.FromResult
