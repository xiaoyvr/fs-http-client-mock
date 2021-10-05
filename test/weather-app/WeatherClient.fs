namespace WeatherApp

open System.Net.Http


type  WeatherClient (client: HttpClient) =
    do
        client.DefaultRequestHeaders.Add("user-agent", "bla")
    member _.Get() =
        client.GetAsync("https://api.weather.gov/")
