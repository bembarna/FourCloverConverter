using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FourCloverConverterBot;

public static class UrlConverterHelper
{
    public static bool UrlIsFccSupported(string url, out string convertFrom)
    {
        string fourChanPattern = @"^https:\/\/i\.4cdn\.org\/([a-zA-Z0-9]+|[0-9]+)\/.+\.webm$";
        string discordPattern = @"^https://cdn\.discordapp\.com/attachments/\d+/\d+/[a-zA-Z0-9!@#$%^&()_+,\-.'\[\]{}~ ]+\.(webm|mp4|mov|avi|mp3|flv|wav|flac|gif)$";
        if (Regex.IsMatch(url, fourChanPattern) || Regex.IsMatch(url, discordPattern))
        {
            convertFrom = url.Split(".").Last();
            return true;
        }

        convertFrom = "";
        return false;
    }
    
    public static bool GetFccUrl(string message, out string url)
    {
        string pattern = @"(?<url>https?://\S+)";
        
        Match match = Regex.Match(message, pattern);
        
        if (match.Success)
        {
            url = match.Groups["url"].Value;
            
            return true;
        }

        url = "";
        return false;
    }
    
    public static async Task<MemoryStream?> ConvertAsync(string url, string convertTo, string convertFrom, string ffmpegPath)
    {
        var fileStream = new MemoryStream();
        if (!CanBuildFfmpegArgsCommand(convertTo, convertFrom, url, out var args))
        {
            return null;
        }
        
        var ffmpegProcess = new Process
        {
            
            StartInfo =
            {
                FileName = ffmpegPath + "\\ffmpeg.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = ffmpegPath
            },
        };
        ffmpegProcess.Start();
        await ffmpegProcess.StandardOutput.BaseStream.CopyToAsync(fileStream);
        await ffmpegProcess.WaitForExitAsync();
        Console.WriteLine($"Output {convertTo} length: {fileStream.Length}");
        fileStream.Position = 0;
        return fileStream;
    }
    
    public static bool CanBuildFfmpegArgsCommand(string convertTo, string convertFrom, string url, out string args)
    {
        if (MediaTypes.ConvertFromUniVideoMediaTypeList.Contains(convertFrom) && MediaTypes.ConvertToMediaTypeListLibvorbisLibvpx.Contains(convertTo))
        {
            args =
                $"-i {url} -movflags faststart -f {convertTo} -preset veryfast -c:v libvpx -crf 28 -b:a 128k -c:a libvorbis -movflags frag_keyframe+empty_moov pipe:1";
            return true;
        }
        else if (MediaTypes.ConvertFromUniVideoMediaTypeList.Contains(convertFrom) && (MediaTypes.ConvertToUniVideoMediaTypeList.Contains(convertTo) || MediaTypes.ConvertToAudioUniMediaTypeList.Contains(convertTo)))
        {
            args = $"-i {url} -movflags faststart -f {convertTo} -preset veryfast -crf 28 -b:a 128k -movflags frag_keyframe+empty_moov pipe:1";
            return true;
        }
        else if (MediaTypes.ConvertFromAudioUniMediaTypeList.Contains(convertFrom) && MediaTypes.ConvertToAudioUniMediaTypeList.Contains(convertTo))
        {
            args = $"-i {url} -movflags faststart -f {convertTo} -preset veryfast -crf 28 -b:a 128k -movflags frag_keyframe+empty_moov pipe:1";
            return true;
        }

        args = "";
        return false;
    }
    
    public static string GenerateRandomGuidString()
    {
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();
        string underscoredGuidString = guidString.Replace("-", "_");
        return underscoredGuidString;
    }
}