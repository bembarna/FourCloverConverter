namespace FourCloverConverterBot;

public static class MessageHelper
{
    public static string GetHelpMessage(string botName, string commandHead, string convertTo)
    {
        return $@"Welcome to {botName}!

You can use this tool to convert video files like webms to mp4s or to rip the audio from a video from mp4 to mp3 or to even convert an mp3 to a wav.

Usage:
{commandHead} <discordMessageAttachmentUrl> <optional: FILE_TYPE default: {convertTo}>
{commandHead} <optional: FILE_TYPE default: {convertTo}> <file attachment 1> <file attachment 2> ...

Available convertible file types:
- Webm
- Flac
- Mp4
- Mov
- Avi
- Wav
- Flv
- Mp3

Examples:
{commandHead} https://cdn.discordapp.com/attachments/0000000000000/000000000/file_name.webm mp4
This will explicitly convert a Discord attachment webm URL to an mp4.

{commandHead} https://cdn.discordapp.com/attachments/0000000000000/000000000/file_name.webm
This will convert a Discord attachment webm URL to the default file type defined by the bot host.
This must be a Discord attachment URL from a Discord message.

{commandHead} mp4
You must add at least 1 attachment with a convertible file type, and this will convert the attachment(s) to an mp4.

{commandHead}
You must add at least 1 attachment with a convertible file type, and this will convert the attachment(s) to the default file type.";
    }
}