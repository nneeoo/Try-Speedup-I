module Files

open System.IO
open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System.Runtime.CompilerServices
open Microsoft.Extensions.Primitives

type ContentType =
    | ImageWebp = 0uy
    | ImageJpg = 1uy
    | Undefined = 255uy

module ContentType =
    let toString =
        function
        | ContentType.ImageJpg -> "image/jpeg"
        | ContentType.ImageWebp -> "image/webp"
        | _ -> " application/octet-stream"

    let ofString (x: string) =
        match x with
        | x when x.EndsWith "webp" -> ContentType.ImageWebp
        | x when x.EndsWith "jpg" || x.EndsWith "jpeg" -> ContentType.ImageJpg
        | _ -> ContentType.Undefined

type ContentEncoding =
    | None = 0uy
    | Deflate = 1uy
    | Gzip = 2uy
    | Br = 3uy

module ContentEncoding =
    let toStringValues =
        function
        | ContentEncoding.None -> StringValues "none"
        | ContentEncoding.Deflate -> StringValues "deflate"
        | ContentEncoding.Gzip -> StringValues "gzip"
        | ContentEncoding.Br
        | _ -> StringValues "br"

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

let extractFileName (x: string) =
    "/" + (x.LastIndexOf "\\" + 1 |> x.Substring)

let inlineFileBytes =
    fun f ->
        let mutable fileBytes = File.ReadAllBytes f

        let mutable fileBytesLongLength =
            fileBytes.LongLength

        let fileLen: LongBytes =
            Unsafe.As &fileBytesLongLength

        let mutable mk = ContentType.ofString f
        let mutable none = ContentEncoding.None

        [| [| Unsafe.As &mk; Unsafe.As &none |]

           [| fileLen.A
              fileLen.B
              fileLen.C
              fileLen.D
              fileLen.E
              fileLen.F
              fileLen.G
              fileLen.H |]

           fileBytes |]
        |> Array.concat
        |> Array.pin

let p =
    (Directory.GetCurrentDirectory(), "wwwroot")
    |> Path.Combine

let files = Directory.GetFiles p

let bytes =
    files |> Array.map inlineFileBytes

let dict = Dictionary()

(files, bytes)
||> Seq.iter2 (fun v i -> dict.Add(extractFileName v, i))

let file (ctx: HttpContext) (next: RequestDelegate) =
    match dict.TryGetValue ctx.Request.Path.Value with
    | false, _ -> next.Invoke ctx
    | true, arr ->
        ctx.Response.ContentType <- Unsafe.As &arr.[0] |> ContentType.toString

        ctx.Response.Headers.ContentEncoding <-
            Unsafe.As &arr.[1]
            |> ContentEncoding.toStringValues

        ctx.Response.ContentLength <- (Unsafe.As &arr.[2]: int64)
        ctx.Response.Body.WriteAsync(arr, 10, Unsafe.As &arr.[2])
