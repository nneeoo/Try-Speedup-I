module Vec

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers


let inline ofString<'a> (x: String) : 'a =
    Unsafe.ReadUnaligned(&Unsafe.As(&Unsafe.Add(&MemoryMarshal.GetReference(x.AsSpan()), -2)))

let cmp (x: String) (y: String) =
    let xx: byte = ofString x
    let yy = ofString y
    xx.CompareTo yy


let tryFindVecIdx (vec: 'a) (arr: 'a array) =
    let rec loop lo hi =
        if lo > hi then
            ValueNone
        else
            let mid: int = lo + hi >>> 1

            match Comparer.Default.Compare(vec, arr.[mid]) with
            | 1 -> loop (mid + 1) hi
            | -1 -> loop lo (mid - 1)
            | _ -> ValueSome mid

    loop 0 arr.Length

let tryFindVec vec arr =
    match Array.BinarySearch(arr, vec) with
    | x when -1 = Int32.Sign x -> ValueNone
    | x -> ValueSome x

let a =
    [| "cc5abf3b653c24feab7ede4f66c72e0"
       "d54dd271e7718c762"
       "0343c1797ae660b681854"
       "f76f9d981c3ddb"
       "80e897b9a165adc3f503078a7c6"
       "e3bf08b79b4b4e1a5db7076ec049c"
       "5f0779142a97c35"
       "d6399354079e"
       "7291201d22c77e5eb709fcb9d99044e786"
       "cc892e5de105225f003a67f"
       "db731f74201d319f2f8a"
       "74bad25c7053d146da829543623396825c0"
       "1a889b6e6adc95028cc48d1e6fd8"
       "6da2f16c1154f588297fac5bd1"
       "1b945f086c4847de763"
       "9af9fa73367"
       "d6b8e2882d034ad4125e7fc86a2d54866"
       "7ba81bce"
       "9e38c5b99e51d89a24e7d209d8069dfb85e0e"
       "ad792cb2fec6b4065bf29c1263505819db37"
       "4f885dcf95335ba99fc447"
       "438516c550a64825f7f0667bc36"
       "7387b9c0637"
       "6f1394f1c3d2abb7be7fdf59a655ba8c034a"
       "fed4bcdbb9c2dee3a3d0b5a"
       "cf7bceb3bb83ba3df3ab9299f77a5c0d"
       "e9e57d38fbd516fdb172e7"
       "8571477481bcfcf32"
       "d373d0653310a85c7b44f"
       "2d1d4a77bcfd1bf37db"
       "d83217006"
       "584237d7790e768ce4" |]

a |> Array.sortInPlaceWith cmp

[<Literal>]
let Times = 65535

[<MemoryDiagnoser; HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)>]
type DictVsInt128Array() =
    let dic = Dictionary(32)
    let a = a |> Array.sortWith cmp

    let vecs: Int128 array =
        a |> Array.map ofString |> Array.sort

    let mutable l = ValueNone
    let mutable j = false, 0

    do
        for i = 0 to a.Length - 1 do
            dic.Add(a.[i], i)

    [<Benchmark>]
    member this.Dictionary() =
        for i = 0 to 31 do
            j <- dic.TryGetValue a.[i]


    [<Benchmark>]
    member this.FsharpBinarySearch() =
        for i = 0 to 31 do
            l <- vecs |> tryFindVecIdx (ofString a.[i])


    [<Benchmark>]
    member this.BCLBinarySearch() =
        for i = 0 to 31 do
            l <- vecs |> tryFindVec (ofString a.[i])

(*
|            Method |     Mean |   Error |  StdDev | CacheMisses/Op | BranchMispredictions/Op |   Gen0 | Allocated |
|------------------ |---------:|--------:|--------:|---------------:|------------------------:|-------:|----------:|
|        Dictionary | 505.6 ns | 5.59 ns | 4.96 ns |             20 |                       1 | 0.0105 |     768 B |
|       Int128Array | 374.2 ns | 3.95 ns | 3.70 ns |              0 |                       0 |      - |         - |
| Int128ArrayDotnet | 443.7 ns | 3.09 ns | 2.74 ns |              0 |                       0 |      - |         - |
*)
