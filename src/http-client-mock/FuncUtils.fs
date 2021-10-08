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
    