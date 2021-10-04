#r "paket:
nuget FSharp.Core 5 
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Core.Trace
nuget Fake.DotNet.Testing.XUnit2 //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.DotNet.Testing
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let buildDir = "./build/"

Target.create "Clean" (fun _ ->
    !! "./src/**/bin/"
        ++ "./src/**/obj/"
        ++ "./test/**/bin/"
        ++ "./test/**/obj/"
        |> Shell.cleanDirs
)

// Default Target
Target.create "Default" (fun _ -> 
    Trace.trace "Hello World from FAKE"
)


Target.create "Build" (fun _ -> 
    !! "./src/**/*.csproj"
    ++ "./test/**/*.csproj"
    |> Seq.iter (DotNet.build id)
)

Target.create "Test" (fun _ ->
    !! "./test/**/bin/**/test.dll"
        |> XUnit2.run (fun p -> 
        {p with
            HtmlOutputPath = Some("./tmp/TestOutput/" @@ "html") })
)


"Clean"
  ==> "Build"
  ==> "Test"

Target.runOrDefault "Default"
