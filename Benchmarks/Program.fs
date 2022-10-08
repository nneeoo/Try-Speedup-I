open System
open System.Net.Http
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Running


[<HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.LlcReference)>]
type HttpBench() =
    let client = new HttpClient()

    let buff: byte array =
        GC.AllocateArray(3348, true)


    [<Benchmark>]
    member this.Base() =
        client
            .GetStreamAsync("http://localhost:5001/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.InMemory() =
        client
            .GetStreamAsync("http://localhost:5002/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.Tables() =
        client
            .GetStreamAsync("http://localhost:5003/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff


    [<Benchmark>]
    member this.Tables_WithoutInterfaces() =
        client
            .GetStreamAsync("http://localhost:5004/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff


    [<Benchmark>]
    member this.Tables_Task() =
        client
            .GetStreamAsync("http://localhost:5005/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.Tables_TaskNoAlloc() =
        client
            .GetStreamAsync("http://localhost:5006/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.Tables_Middleware() =
        client
            .GetStreamAsync("http://localhost:5007/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.InlineEverything() =
        client
            .GetStreamAsync("http://localhost:5008/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff
            
    [<Benchmark>]
    member this.FsharpFinal() =
        client
            .GetStreamAsync("http://localhost:5009/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff

    [<Benchmark>]
    member this.CsharpFinal() =
        client
            .GetStreamAsync("http://localhost:5010/Lenna-3.webp")
            .GetAwaiter()
            .GetResult()
            .ReadExactly buff
            
BenchmarkRunner.Run<HttpBench>()
