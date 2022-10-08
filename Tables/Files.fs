module Files

open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

let p =
    (Directory.GetCurrentDirectory(), "wwwroot")
    |> Path.Combine

let dict = Dictionary()
let extractMime _ = "image/webp"
let extractFileName (x: string) = x.LastIndexOf "\\" + 1 |> x.Substring
let files = p |> Directory.GetFiles
let mimes = files |> Array.map extractMime

let encodings =
    Array.init p.Length (fun _ -> StringValues "none")

let bytes =
    files |> Array.map File.ReadAllBytes

files
|> Array.map extractFileName
|> Seq.iteri (fun i v -> dict.Add(v, i))


type FileResult(mime, encoding, stream: byte array) =
    interface IResult with
        member this.ExecuteAsync(ctx: HttpContext) =
            ctx.Response.ContentType <- mime
            ctx.Response.Headers.ContentEncoding <- encoding
            ctx.Response.ContentLength <- stream.LongLength
            ctx.Response.Body.WriteAsync(stream, 0, stream.Length)

let FileResult x = FileResult x :> IResult

type IResultExtensions with
    member this.FileResult = FileResult


let file (i: string) =
    match dict.TryGetValue i with
    | false, _ -> Results.NotFound()
    | true, idx -> Results.Extensions.FileResult(mimes.[idx], encodings.[idx], bytes.[idx])
