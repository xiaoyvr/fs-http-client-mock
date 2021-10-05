namespace WeatherApp.Controllers

open System.Net.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Net.Http.Json
open WeatherApp


//type  WeatherClient (client: HttpClient) =
//    do
//        client.DefaultRequestHeaders.Add("user-agent", "bla")
//    member _.Get() =
//        client.GetAsync("https://api.weather.gov/")
           
[<ApiController>]
[<Route("[controller]")>]
type WeatherController (logger : ILogger<WeatherController>, client : WeatherClient) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        async {
            let! response = client.Get() |> Async.AwaitTask
            let! body = response.Content.ReadFromJsonAsync<{|status: string|}>() |> Async.AwaitTask
            return body.status
        }
