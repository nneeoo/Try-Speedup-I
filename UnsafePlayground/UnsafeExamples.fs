module UnsafePlayground

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop
open NUnit.Framework

[<SetUp>]
let Setup () = ()

[<Struct>]
type LongBytes =
    { A: byte
      B: byte
      C: byte
      D: byte
      E: byte
      F: byte
      G: byte
      H: byte }

[<Test>]
let Int64AsBytes () =
    let mutable i = 1337L
    let res: LongBytes = Unsafe.As &i
    printfn "%O" res

    Assert.Pass()


[<Test>]
let Int64AsInt32 () =
    let mutable i = 1337L
    let res: int = Unsafe.As &i
    Assert.AreEqual(res, 1337)

[<Test>]
let StringAsByteArray () =
    let mutable s = "abcd"

    let res: byte array = Unsafe.As &s

    for idx = 0 to s.Length - 1 do
        printfn "%O" res.[idx]

    Assert.Pass()


[<Test>]
let StringAsByteArray2 () =
    let mutable s = "abcd"
    let res: byte array = Unsafe.As &s

    for i in res do
        printfn "%O" i

    Assert.Pass()


//[<Test>]
let StringAsByteArray3 () =
    let mutable s = "abcd"
    let res: byte array = Unsafe.As &s

    let re = res.GetEnumerator()

    while re.MoveNext() do
        printfn "%O" re.Current

    Assert.Pass()

[<Test>]
let StringAsByteArray4 () =
    let mutable s = "a"
    let res: byte array = Unsafe.As &s

    res.LongLength |> printfn "%O"

    Assert.Pass()

let unsafeLongLen x : int64 =
    Unsafe.ReadUnaligned(&Unsafe.Add(&MemoryMarshal.GetArrayDataReference x, -8))

[<Test>]
let StringAsByteArray5 () =
    let mutable s = " "
    let res: byte array = Unsafe.As &s

    res.Length |> printfn "%O"
    res.LongLength |> printfn "%O"
    res |> unsafeLongLen |> printfn "%O"

    let mutable len: int64 =
        res |> unsafeLongLen

    let longBytes: LongBytes = Unsafe.As &len
    printfn "%O" longBytes

    Assert.Pass()


[<Test>]
let MutateString () =
    let mutable s = "abcde"
    let mutable res: char array = Unsafe.As &s

    let ref =
        &MemoryMarshal.GetArrayDataReference res

    // заголовок массива это 8 байт
    // заголовок строки это 4 байта
    // нужно сдвинуть поинтер влево
    // еще на 2 char'a, то есть на 4 байта
    for i = -2 to s.Length - 3 do
        Unsafe.Add(&ref, i) <- '-'

    printfn "%s" s
    Assert.AreEqual("-----", s)
