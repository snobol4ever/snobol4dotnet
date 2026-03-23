namespace FSharpLibrary

open Snobol4.Common
open System.Collections.Generic

/// Helper: call Var.Convert using mutable F# refs for the out params
module private Conv =
    let asInt (v: Var) (exec: Executive) =
        let mutable vOut = Unchecked.defaultof<Var>
        let mutable oOut = Unchecked.defaultof<obj>
        let ok = v.Convert(Executive.VarType.INTEGER, &vOut, &oOut, exec)
        if not ok then failwith "expected integer"
        oOut :?> int64

    let asReal (v: Var) (exec: Executive) =
        let mutable vOut = Unchecked.defaultof<Var>
        let mutable oOut = Unchecked.defaultof<obj>
        let ok = v.Convert(Executive.VarType.REAL, &vOut, &oOut, exec)
        if not ok then failwith "expected real"
        oOut :?> double

    let asString (v: Var) (exec: Executive) =
        let mutable vOut = Unchecked.defaultof<Var>
        let mutable oOut = Unchecked.defaultof<obj>
        let ok = v.Convert(Executive.VarType.STRING, &vOut, &oOut, exec)
        if not ok then failwith "expected string"
        oOut :?> string

/// F# external library — proves IExternalLibrary works from F#.
/// Registers: FsFib, FsIsPalindrome, FsJoinWith, FsHypot
type StringFunctions() =
    interface IExternalLibrary with
        member _.Init(exec: Executive) =
            let reg (name: string) (arity: int) (fn: List<Var> -> unit) =
                let key = exec.Parent.FoldCase(name)
                exec.FunctionTable[key] <-
                    FunctionTableEntry(exec, key, FunctionTableEntry.FunctionHandler(fn), arity, false)

            // FsFib(n) — recursive Fibonacci → IntegerVar
            let fsFib (args: List<Var>) =
                let n = Conv.asInt args[0] exec
                let rec fib i = if i <= 1L then i else fib(i-1L) + fib(i-2L)
                exec.SystemStack.Push(IntegerVar(fib n))

            // FsIsPalindrome(s) — predicate: :S if s = reverse(s)
            let fsIsPalindrome (args: List<Var>) =
                let s = Conv.asString args[0] exec
                let rev = System.String(s |> Seq.rev |> Seq.toArray)
                if s = rev then exec.PredicateSuccess() |> ignore
                else exec.NonExceptionFailure() |> ignore

            // FsJoinWith(sep, items) — join space-delimited tokens with sep → StringVar
            let fsJoinWith (args: List<Var>) =
                let sep   = Conv.asString args[0] exec
                let items = (Conv.asString args[1] exec).Split(' ')
                            |> Array.filter (fun s -> s.Length > 0)
                exec.SystemStack.Push(StringVar(System.String.Join(sep, items))) |> ignore

            // FsHypot(a, b) — sqrt(a²+b²) → RealVar
            let fsHypot (args: List<Var>) =
                let a = Conv.asReal args[0] exec
                let b = Conv.asReal args[1] exec
                exec.SystemStack.Push(RealVar(System.Math.Sqrt(a*a + b*b)))

            reg "FsFib"          1 fsFib
            reg "FsIsPalindrome" 1 fsIsPalindrome
            reg "FsJoinWith"     2 fsJoinWith
            reg "FsHypot"        2 fsHypot

        member _.Unload() = ()
