#r "paket:
nuget FSharp.Core 6.0.0
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Core.Trace //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let DotNetExec cmd = 
    DotNet.exec id cmd ""

Target.create "Clean" (fun _ ->
    !! "./src/**/bin/"
        ++ "./src/**/obj/"
        ++ "./test/**/bin/"
        ++ "./test/**/obj/"
        |> Shell.cleanDirs
)

Target.create "Default" (fun _ -> 
    Trace.trace "Hello World from FAKE"
)


Target.create "Build" (fun _ -> 
    !! "./src/**/*.csproj"
    ++ "./test/**/*.csproj"
    |> Seq.iter (DotNet.build id)
)

Target.create "Test" (fun _ ->
    DotNetExec "test" |> ignore
)


// opening with https://forums.fsharp.org/t/constraining-type-definition-to-enable-fleece-de-serialization/1028

// step0: quick intro about paket, fake, |> operator and unit  

// ------------------ begin ----------------


(*
The problem: lots of boilerplate code to make sure the exit code is being handled properly in our build script. 
when we have more steps, the code will smell worse. 
*)

// let DotNetExec cmd = 
//     DotNet.exec id cmd ""

Target.create "TestExitCode0" (fun _ ->
    let result1 = DotNetExec "build"
    if result1.ExitCode = 0 then
        failwith "Build failed. "
    let result2 = DotNetExec "test"
    if result2.ExitCode = 0 then
        failwith "Test failed. "
)

(*
Lets chain it together to reduce the boilerplate code, 
since we don't have a function to take ProcessResult as the last parameter, we created another version 
of our function DotNetExec2 to take the extra parameter. 
*)
        
let failWithExitCode (p:ProcessResult) =
    if p.ExitCode <> 0 then failwith "Process execution failed. "
    
let DotNetExec2 str (pr:ProcessResult) =
    if pr.ExitCode = 0 then
        pr
    else
        DotNetExec str

Target.create "TestExitCode01" (fun _ ->
    DotNetExec "build" |> DotNetExec2 "test" |> failWithExitCode
)

(*
code smell: 2 method has similar names, can we make it more clear?
let's use high order function to alter the original function instead of create another one
*)

let addExtraParam (f: string -> ProcessResult) =
    fun str (p:ProcessResult) ->
        if p.ExitCode <> 0 then
            p
        else
           f str
           
Target.create "TestExitCode1" (fun _ ->
    DotNetExec "build" |> addExtraParam DotNetExec "test" |> failWithExitCode
)


(*
code smell: addExtraParam bound with string and ProcessResult, can we make it more generic? 
so that other processes take different parameter or have a different return structure can also use it. 
let's make it more generic with type parameter and SRTP (Statically Resolved Type Parameters)
*)
let inline addExtraParamGeneric<'T, ^PR when ^PR: (member ExitCode: int)> (f: 'T -> ^PR) =
    fun t (p:^PR) ->
        let exitCode = (^PR :(member ExitCode: int) p)
        if exitCode <> 0 then
            p
        else
           f t

Target.create "TestExitCode2" (fun _ ->
    DotNetExec "build" |> addExtraParamGeneric DotNetExec "test" |> failWithExitCode
)

(*
let's use operator overload to make it more compact
*)
let (!^+) = addExtraParamGeneric

Target.create "TestExitCode21" (fun _ ->
    DotNetExec "build" |> !^+ DotNetExec "test" |> failWithExitCode
)

(*
only use the original function with an generic operator overload, pretty good, right?
 
now, the problem is, we lost our exit reason while we chain the method together. 
but we still want to report which step it failed. let's use a construct to capture it.
*)    
type Exitable =
    | Exit of string
    | Continue
                
let ExitableDotNetExec cmd = 
    let r = DotNet.exec id cmd ""
    if r.ExitCode = 0 then
        Continue
    else
        Exit($"Exec {cmd} failed.")            

let failExitable exitable =
    match exitable with
        | Exit s -> failwith s |> ignore
        | _ -> ()
        
let liftAsExitable f =
    fun s exitable ->
        match exitable with
            | Exit s -> Exit(s)
            | _ -> f s

Target.create "TestExitCode3" (fun _ ->
    ExitableDotNetExec "build" |> liftAsExitable ExitableDotNetExec "test" |> failExitable
)

(*
You can still use operator overload to make the code more compact.
 
now the problem is, liftAsExitable bound with the signature of ('a -> Exitable), what if there is another function take 
2 or more parameters? can we make the liftAsExitable more generic reusable, so that other functions, with more parameters,  
returns an Exitable, can also use it?

let's introduce a more generic bind method, take an exitable and an (Unit -> Exitable)
*)
type Exitable with
    static member bind (e, f)  =
        match e with
            | Exit s -> Exit(s)
            | Continue -> f ()

(*
Now the bind function can take a function takes a Unit, which means takes nothing, and returns a Exitable. now let's  
convert our function invocation to it. let's call it ```later```.
*)

let later1 f p1 = fun() -> f p1
let (!/>) = later1
//let ExitableDotNetExecLater = later1 ExitableDotNetExec
            
Target.create "TestExitCode31" (fun _ ->
    Exitable.bind (Exitable.bind (Continue, !/> ExitableDotNetExec "build"), !/> ExitableDotNetExec "test")   |> failExitable
)

(*
Now the bind function can take a function takes a Unit, which means takes nothing, and returns a Exitable. All 
function invocation can be convert to it.
  
But it is hard to read. Again, let's make it more readable by operator overload.
*)
let (>==>) m f = Exitable.bind(m, f)

Target.create "TestExitCode32" (fun _ ->
    Continue >==> !/> ExitableDotNetExec "build" >==> !/> ExitableDotNetExec "test" |> failExitable
    // can also be
//    !/> ExitableDotNetExec "build" () >==> !/> ExitableDotNetExec "test" |> failExitable
)

(* 
operator >==> now bound with Exitable and (Unit -> Exitable). since operator is more valuable (we only have so many). 
we should make it more generic, so that other similar types have a bind method can also use this operator. 
let's use SRTP, again
*)
let inline bindGeneric<'v, ^M  when ^M: (static member bind: ^M -> ('v -> ^M) -> ^M)> (m:^M) (f: 'v -> ^M) : ^M =    
    (^M :(static member bind: ^M -> ('v -> ^M) -> ^M) m,f )
    
let (>>=) = bindGeneric
    
Target.create "TestExitCode33" (fun _ ->
    Continue >>= !/> ExitableDotNetExec "build" >>= !/> ExitableDotNetExec "test" |> failExitable
    // can also be
    // !/> ExitableDotNetExec "build" () >>= !/> ExitableDotNetExec "test" |> failExitable
)

(*
Now we decoupled (>>=) with Exitable. it is more generic. it should be able to chain anything together, as long as 
"the thing" has a static bind method. use your imagination. 

Wait, is this a ... monad ??? 

lets verify with 3 laws (<==> means is the same with)
1. left identity: ```return x >>= f``` <==> ```f x```
2. right identity: ```m >>= return``` <==> ```m```
3. associativity: ```(m >>= f) >>= g```  <==> ```m >>= (fun x -> f x >>= g)```

in our case, the type constructor (m) is Exitable , the underlying type (x) is Unit

lets define the return as:
let return () = Continue

law 1: 
return () >>= f
<==> bind (return (), f)
<==> bind (Continue, f)
<==> f ()

law 1 passed!

law 2:
assume m is Continue:
m >>= return
<==> bind (m, return)
<==> bind (Continue, return) 
<==> return () 
<==> Continue 
<==> m

assume m is Exit(msg):
bind (m, return)
<==> bind (Exit(msg), return) 
<==> Exit(msg) 
<==> m

law 2 also passed!

law 3:
assume m is Continue
left side: (m >>= f) >>= g
<==> (bind (bind Continue, f), g)
<==> (bind f(), g)

right side: m >>= (fun () -> f () >>= g)
<==> (bind Continue, (fun () -> f () >>= g))
<==> (bind Continue, (fun () -> (bind f (), g)))
<==> (fun () -> (bind f (), g)) ()
<==> (bind f(), g)

left side <==> right side

assume m is Exit(msg)
left side: (m >>= f) >>= g
<==> (bind (bind Exit(msg), f), g)
<==> Exit(msg)

right side: m >>= (fun x -> f x >>= g)
<==> (bind Exit(msg), (fun () -> f () >>= g))
<==> Exit(msg)

left side <==> right side 

3 laws all passed! We really got a monad, people!!!


Monad has another syntax construct in F#, computation expression. 
Let's try to use computation expression to finish our exercise.
*) 

type ExitableBuilder() =
    member this.Bind(m, f) =
        match m with
            | Exit s -> Exit(s)
            | Continue -> f ()

    member this.Zero() =
        Continue
    
let exitable = ExitableBuilder()
Target.create "TestExitCode4" (fun _ ->
    exitable {
        do! ExitableDotNetExec "build"
        do! ExitableDotNetExec "test"
    } |> failExitable
)

(*
closing:
* pipe expressions instead of write sequence of instructions
* use high order function to alter the original function to make the pipe
* use operator overload to make the pipe looks more compact
* make operator as generic as possible, so that it can be reused
* lift the original type to Union to pass information through the pipe
* define bind (>>=) to express the computation through the pipe 
* can also choose to use computation expression instead of pipe to make it more readable

How deep you can go to solve a really trivial problem?
*)

// ------------------the end ----------------

"Clean"
  ==> "Build"
  ==> "Test"

Target.runOrDefault "Default"
