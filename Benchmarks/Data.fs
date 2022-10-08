module Benchmarks.Data

open System.Net.Http
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http



let client = new HttpClient()

let inline get () =
    client
        .GetStreamAsync("http://localhost:5000/Lenna-3.webp")
        .GetAwaiter()
        .GetResult()


[<MemoryDiagnoser; HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)>]
type HttpBench() =

    let mutable r = Results.NotFound()
    let mutable t = Task.CompletedTask

    [<Benchmark>]
    member this.InMemory() = get ()


