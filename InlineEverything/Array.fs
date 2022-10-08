module Array

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


let inline unsafeTakeAt<'a> (idx: int) (arr: 'a array) =
    let mutable ptr =
        &MemoryMarshal.GetArrayDataReference arr

    &Unsafe.Add(&ptr, idx)

let inline unsafeLength arr : int64 =
    Unsafe.As(&Unsafe.Add(&MemoryMarshal.GetArrayDataReference arr, 8))

let pin (i: 'a array) =
    let out =
        GC.AllocateArray<'a>(i.Length, true)

    Array.Copy(i, out, i.Length)
    out
