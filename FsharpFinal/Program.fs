open System.Runtime
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
            i.ListenAnyIP(5009)
            i.AddServerHeader <- false)
        |> ignore

        prod.Logging.ClearProviders() |> ignore
        prod

    | dev ->
        dev.Logging.AddConsole().AddDebug() |> ignore
        dev

|> fun b -> b.Build()
|> fun w ->
    w.Use Files.file |> ignore
    w.UseRouting() |> ignore
    w.Run()
