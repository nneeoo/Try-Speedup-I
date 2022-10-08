using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsharpFinal;

internal struct LongBytes
{
#pragma warning disable CS0649
    public byte A;
    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte F;
    public byte G;
    public byte H;
#pragma warning restore CS0649
}

public static class ContentHelper
{
    public static byte[] InlineBytes(string literalPath)
    {
        var bytes = File.ReadAllBytes(literalPath);
        var contentByte = (byte)FileHelper.ParseContentType(literalPath);
        const byte encodingByte = (byte)ContentEncoding.None;
        var bytesLongLength = bytes.LongLength;

        var lenBytes = Unsafe.As<Int64, LongBytes>(ref bytesLongLength);
        var content = new[]
        {
            contentByte, encodingByte, lenBytes.A, lenBytes.B, lenBytes.C, lenBytes.D, lenBytes.E, lenBytes.F,
            lenBytes.G, lenBytes.H
        };
        var concat = content.Concat(bytes).ToArray();
        var res = GC.AllocateArray<byte>(concat.Length, true);

        Array.Copy(concat, res, res.Length);
        return res;
    }
}

public class FileEndpoint
{
    private readonly Dictionary<string, byte[]> _files = new();

    public FileEndpoint(string path)
    {
        Path = path;
    }

    private string Path { get; }

    public FileEndpoint Initialize()
    {
        foreach (var s in Directory.GetFiles(Path))
        {
            var bytes = ContentHelper.InlineBytes(s);
            var key = "/" + s[(s.LastIndexOf("\\", StringComparison.Ordinal) + 1)..];
            _files.Add(key, bytes);
        }

        return this;
    }

    public Task TryFetchFile(HttpContext ctx, RequestDelegate next)
    {
        Debug.Assert(ctx.Request.Path.Value != null, "ctx.Request.Path.Value != null");
        if (!_files.TryGetValue(ctx.Request.Path.Value, out var buffer)) return next.Invoke(ctx);

        ref var reference = ref Unsafe.Add(ref buffer.AsSpan().GetPinnableReference(), 2);
        ctx.Response.ContentType = FileHelper.ContentTypeToString(Unsafe.ReadUnaligned<ContentType>(ref buffer[0]));

        ctx.Response.Headers.ContentEncoding =
            FileHelper.ContentEncodingToString(Unsafe.ReadUnaligned<ContentEncoding>(ref buffer[1]));
        ctx.Response.ContentLength = Unsafe.ReadUnaligned<long>(ref reference);

        var fileSize = Unsafe.ReadUnaligned<int>(ref reference);
        return ctx.Response.Body.WriteAsync(buffer, 10, fileSize);
    }
}