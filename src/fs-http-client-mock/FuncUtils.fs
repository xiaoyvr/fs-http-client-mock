module private HttpClientMock.FuncUtils

open System

let ignoreOption f =
    Func<_>(fun () -> match f() with
                        | Some r -> r
                        | None -> Unchecked.defaultof<_>
                        )

let ignoreTupleOption f =
    Func<Tuple<_,_>>(fun () -> match f() with
                                        | Some (r, Some(c)) -> (r, c)
                                        | Some (r, None) -> (r, Unchecked.defaultof<_>)
                                        | None -> Unchecked.defaultof<_>
                                        )
let rec tryFindMap<'T, 'TM> (mapFun: 'T -> 'TM option) (seq: 'T seq) =
    match seq |> List.ofSeq with
        | [] -> None
        | head :: tail ->
            match mapFun(head) with
                | Some t -> (Some (t, head))
                | None -> tryFindMap mapFun tail
                
let rec tryFindMapBack<'T, 'TM> (mapFun: 'T -> 'TM option) (seq: 'T seq) =
    (Seq.rev >> tryFindMap mapFun) seq