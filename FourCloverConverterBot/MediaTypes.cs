namespace FourCloverConverterBot;

public static class MediaTypes
{
    //requires -c:v libvpx and -c:a libvorbis to convert to it
    public static string Webm { get; } = "webm";
    
    //everything below can be converted into each other and vice-versa
    
    public static string Flv { get; } = "flv";

    public static string Mp4 { get; } = "mp4";
    
    public static string Mov { get; } = "mov";

    public static string Avi { get; } = "avi";

    //Audi
    public static string Mp3 { get; } = "mp3";

    public static string Wav { get; } = "wav";
    
    public static string Flac { get; } = "flac";
    
    //Image
    public static string Gif { get; } = "gif";


    //broken under, investigate later 
    
    public static string Aiff { get; } = "aiff";

    public static string Aac { get; } = "aac";

    public static string Mkv { get; } = "mkv";
    
    public static string M4a { get; } = "m4a";

    public static string Wma { get; } = "wma";
    
    public static readonly List<string> ConvertableFromMediaTypeList = new List<string>()
    {
        Webm,
        Flac,
        Mp4,
        Mov,
        Avi,
        Wav,
        Flv,
        Mp3
    };
    
    public static readonly List<string> MediaTypeList = new List<string>()
    {
        Webm,
        Gif,
        Flac,
        Mp4,
        Mov,
        Avi,
        Wav,
        Flv,
        Mp3
    };
    
    public static readonly List<string> ConvertToUniMediaTypeList = new List<string>()
    {
        Gif,
        Flac,
        Mp4,
        Mov,
        Avi,
        Wav,
        Flv,
        Mp3
    };
    
    public static readonly List<string> ConvertFromUniVideoMediaTypeList = new List<string>()
    {
        Webm,
        Mp4,
        Mov,
        Avi,
        Flv
    };
    
    public static readonly List<string> ConvertToUniVideoMediaTypeList = new List<string>()
    {
        Mp4,
        Mov,
        Avi,
        Flv,
        Gif
    };
    
    public static readonly List<string> ConvertToAudioUniMediaTypeList = new List<string>()
    {
        Flac,
        Wav,
        Mp3
    };
    
    public static readonly List<string> ConvertFromAudioUniMediaTypeList = new List<string>()
    {
        Flac,
        Wav,
        Mp3
    };
    
    public static readonly List<string> ConvertToMediaTypeListLibvorbisLibvpx = new List<string>()
    {
        Webm
    };
}