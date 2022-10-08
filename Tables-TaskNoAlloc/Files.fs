module Files

open System.Collections.Generic
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

let p =
    (Directory.GetCurrentDirectory(), "wwwroot")
    |> Path.Combine

let dict = Dictionary()
let extractMime _ = "image/webp"

//let extractFileName (x: string) = x.LastIndexOf "\\" + 1 |> x.Substring
let extractFileName (x: string) =
    "/" + (x.LastIndexOf "\\" + 1 |> x.Substring)

let files = p |> Directory.GetFiles
let mimes = files |> Array.map extractMime

let encodings =
    Array.init p.Length (fun _ -> StringValues "none")

let bytes =
    files |> Array.map File.ReadAllBytes

files
|> Array.map extractFileName
|> Seq.iteri (fun i v -> dict.Add(v, i))

//let file (i: string) (ctx: HttpContext) =
let file (ctx: HttpContext) =
    //match dict.TryGetValue i with
    match dict.TryGetValue ctx.Request.Path.Value with
    | false, _ ->
        ctx.Response.StatusCode <- 404
        Task.CompletedTask
    | true, idx ->
        ctx.Response.ContentType <- mimes.[idx]
        ctx.Response.Headers.ContentEncoding <- encodings.[idx]
        let b = bytes.[idx]
        ctx.Response.ContentLength <- b.LongLength
        ctx.Response.Body.WriteAsync(b, 0, b.Length)
