module Bench

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers
open Microsoft.FSharp.NativeInterop

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let inline unsafeTakeAt (arr: 'a array) (idx: int) =
    Unsafe.Add(&MemoryMarshal.GetArrayDataReference arr, idx)


[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let inline unsafe x =
    Unsafe.ReadUnaligned(&Unsafe.Add(&MemoryMarshal.GetArrayDataReference x, -8))

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let inline unsafe2 x =
    Unsafe.As(&Unsafe.Add(&MemoryMarshal.GetArrayDataReference x, -8))

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let unsafe3 (x: 'a array) : int64 =
    use q = fixed x

    NativePtr.add q -8
    |> NativePtr.toVoidPtr
    |> Unsafe.ReadUnaligned


[<MemoryDiagnoser; HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)>]
type FieldVsUnsafe() =
    let mutable b = [||].LongLength
    let mutable l = 0

    let arr =
        [| for i = 0 to 1025 do
               [| for j = 0 to i do
                      0uy |] |]

    do
        for i = 0 to arr.Length - 1 do
            if
                arr.[i].LongLength <> unsafe arr.[i]
                || arr.[i].LongLength <> unsafe2 arr.[i]
            then
                failwith "!"

    [<Benchmark>]
    member this.Unsafe() =
        for i = 0 to arr.Length - 1 do
            b <- unsafe arr.[i]

    [<Benchmark>]
    member this.Unsafe2() =
        for i = 0 to arr.Length - 1 do
            b <- unsafe2 arr.[i]

    [<Benchmark>]
    member this.UnsafeInt() =
        for i = 0 to arr.Length - 1 do
            l <- unsafe arr.[i]

    [<Benchmark>]
    member this.Unsafe3() =
        for i = 0 to arr.Length - 1 do
            b <- unsafe3 arr.[i]


    [<Benchmark(Baseline = true)>]
    member this.StandardInt() =
        for i = 0 to arr.Length - 1 do
            l <- arr.[i].Length

    [<Benchmark>]
    member this.StandardLong() =
        for i = 0 to arr.Length - 1 do
            b <- arr.[i].LongLength
