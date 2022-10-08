module Files

open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

type File =
    { Bytes: byte array
      ContentEncoding: StringValues
      Mime: string }

let p =
    (Directory.GetCurrentDirectory(), "wwwroot")
    |> Path.Combine

let extractMime x = "image/webp"
let extractFileName (x: string) = x.LastIndexOf "\\" + 1 |> x.Substring
let dict = Dictionary()

p
|> Directory.GetFiles
|> Seq.iter (fun i ->
    (extractFileName i,
     { Bytes = File.ReadAllBytes i
       ContentEncoding = StringValues "none"
       Mime = extractMime i })
    |> dict.Add)

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
    | true, x -> Results.Extensions.FileResult(x.Mime, x.ContentEncoding, x.Bytes)
