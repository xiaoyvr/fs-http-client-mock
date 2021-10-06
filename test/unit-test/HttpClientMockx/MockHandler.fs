namespace HttpClientMockx

open System.Net
open System.Net.Http
open System.Threading.Tasks


type Behavior = {Matcher: HttpRequestMessage -> bool; Responder: HttpRequestMessage -> HttpResponseMessage}

type MockHandler(behaviors: Behavior[]) =
    inherit DelegatingHandler()
    
    override this.SendAsync(request: HttpRequestMessage, _: System.Threading.CancellationToken) =
        let httpResponseMessage = match behaviors |> Seq.tryFind(fun b -> b.Matcher(request)) with
                                    | None -> new HttpResponseMessage(HttpStatusCode.NotFound)
                                    | Some(b) -> b.Responder(request)
        if request.Method = HttpMethod.Head then
            httpResponseMessage.Content <- null
        Task.FromResult(httpResponseMessage)
