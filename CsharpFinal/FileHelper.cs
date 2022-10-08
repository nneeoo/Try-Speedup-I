namespace CsharpFinal;

public enum ContentType : byte
{
    ImageWebp = 0,
    ImageJpg = 1,
    Undefined = 100
}

public enum ContentEncoding : byte
{
    None = 0,
    Deflate = 1,
    Gzip = 100,
    Br = 3
}

public static class FileHelper
{
    public static ContentType ParseContentType(string arg1)
    {
        if (arg1.EndsWith("webp")) return ContentType.ImageWebp;
        if (arg1.EndsWith("jpg") || arg1.EndsWith("jpeg")) return ContentType.ImageJpg;
        return ContentType.Undefined;
    }

    public static string ContentTypeToString(ContentType arg1)
        => arg1 switch
        {
            ContentType.ImageWebp => "image/webp",
            ContentType.ImageJpg => "image/jpeg",
            _ => "application/octet-stream"
        };
    
    public static string ContentEncodingToString(ContentEncoding arg1)
        => arg1 switch
        {
            ContentEncoding.Deflate => "deflate",
            ContentEncoding.Gzip => "gzip",
            ContentEncoding.Br => "br",
            ContentEncoding.None => "none",
            _ => "none"
        };
}