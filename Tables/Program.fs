open System
open System.Runtime
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting

GCSettings.LatencyMode <- GCLatencyMode.SustainedLowLatency

WebApplicationOptions(WebRootPath = Files.p)
|> WebApplication.CreateBuilder
|> function
    | prod when prod.Environment.IsProduction() ->
        prod.WebHost.UseKestrel(fun i ->
            i.ListenAnyIP(5003)
            i.AddServerHeader <- false)
        |> ignore

        prod.Logging.ClearProviders() |> ignore
        prod

    | dev ->
        dev.Logging.AddConsole().AddDebug() |> ignore
        dev

|> fun b -> b.Build()
|> fun w ->
    w.MapGet("/{i}", Func<string, IResult>(fun i -> Files.file i))
    |> ignore

    w.Run()
