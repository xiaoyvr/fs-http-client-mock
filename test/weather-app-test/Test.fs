namespace WeatherApp.Test

open System.Net
open HttpClientMock
open System.Net.Http
open Microsoft.AspNetCore.Hosting
open Xunit
open Microsoft.AspNetCore.TestHost
open WeatherApp
open Microsoft.Extensions.DependencyInjection
open FsUnit.Xunit

module Test =

    module TestServer =
        let create (b: IWebHostBuilder) =
            new TestServer(b)
            
    
    let convf<'T> (func: System.Func<'T>) =
        fun () -> func.Invoke()
        
    let createTestServer (customize : (IServiceCollection -> 'a)) =
        WebHostBuilder()
            .UseStartup<WeatherApp.Startup>()
            .ConfigureTestServices( fun c -> customize(c) |> ignore)
        |> TestServer.create        
        
    
    [<Fact>]
    let ``should get the status of weather api``() =
        
        let b = MockedHttpClientBuilder()
        let capture = b.WhenGet("/")
                          .Respond(HttpStatusCode.OK, {|status = "Bla"|})
                          .Capture() |> convf
        let mock = b.Build("https://api.weather.gov")
        
        
        use server = createTestServer(fun sc -> sc.AddHttpClient(fun _ -> WeatherClient(mock))  )
        use client = server.CreateClient()
        
        let result = 
            async {
                let! response = client.GetAsync("/weather") |> Async.AwaitTask
                let! result = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return result
            } |> Async.RunSynchronously
            
        result |>  should equal "Bla"
         
        let requestCapture = capture()
        requestCapture.Method |> should equal HttpMethod.Get.Method
