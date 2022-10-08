module Files

open System.Collections.Generic
open System
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

let p =
    (Directory.GetCurrentDirectory(), "wwwroot")
    |> Path.Combine

let dict = Dictionary()
let extractMime _ = "image/webp"

let extractFileName (x: string) =
    "/" + (x.LastIndexOf "\\" + 1 |> x.Substring)

let enc = UTF8Encoding false

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

let inlineFileBytes =
    fun f ->
        let mimeBytes =
            extractMime f |> enc.GetBytes

        let mutable mimeBytesLongLength =
            mimeBytes.LongLength

        let mimeLen: LongBytes =
            Unsafe.As(&mimeBytesLongLength)

        let encodingBytes = "none" |> enc.GetBytes

        let mutable mimeBytesLongLength =
            encodingBytes.LongLength

        let encLen: LongBytes =
            Unsafe.As(&mimeBytesLongLength)

        let mutable fileBytes = File.ReadAllBytes f

        let mutable fileBytesLongLength =
            fileBytes.LongLength

        let fileLen: LongBytes =
            Unsafe.As(&fileBytesLongLength)


        [| [| mimeLen.A
              mimeLen.B
              mimeLen.C
              mimeLen.D
              mimeLen.E
              mimeLen.F
              mimeLen.G
              mimeLen.H |]
           mimeBytes

           [| encLen.A
              encLen.B
              encLen.C
              encLen.D
              encLen.E
              encLen.F
              encLen.G
              encLen.H |]
           encodingBytes

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


let files = p |> Directory.GetFiles

let bytes =
    files |> Array.map inlineFileBytes

(files, bytes)
||> Seq.iter2 (fun v i -> dict.Add(extractFileName v, i))


let file (ctx: HttpContext) (next: RequestDelegate) =
    match dict.TryGetValue ctx.Request.Path.Value with
    | false, _ -> next.Invoke ctx
    | true, arr ->
        let spanRef =
            &arr.AsSpan().GetPinnableReference()

        let mimeLen: int32 =
            Unsafe.ReadUnaligned(&spanRef)

        ctx.Response.ContentType <- enc.GetString(arr, 8, mimeLen)

        let encStart = mimeLen + 8

        let encLen: int32 =
            Unsafe.ReadUnaligned(&Unsafe.Add(&spanRef, encStart))

        ctx.Response.Headers.ContentEncoding <- enc.GetString(arr, encStart + 8, encLen)

        let fileStart = encStart + encLen + 8

        let mutable fileSize: int64 =
            Unsafe.ReadUnaligned(&Unsafe.Add(&spanRef, fileStart))

        ctx.Response.ContentLength <- fileSize

        ctx.Response.Body.WriteAsync(arr, fileStart + 8, Unsafe.As(&fileSize))
