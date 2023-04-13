using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FourCloverConverterBot;

public class DiscordSocketClientHelper
{
    private DiscordSocketClient Client { get; set; }

    public DiscordSocketClientHelper(DiscordSocketClient client)
    {
        Client = client;
    }
    
    public async Task<RestUserMessage?> SendMessageFromMessageChannelId(ulong messageChannelId, string message, bool removeMessageAfterSending = false)
    {
        var textChannel = Client.GetChannel(messageChannelId) as SocketTextChannel;

        if (textChannel is null)
        {
            return null;
        }
        
        var sentMessage = await textChannel.SendMessageAsync(message);
        
        if (removeMessageAfterSending)
        {
            await sentMessage.DeleteAsync();
            return null;
        }

        return sentMessage;
    }
    
    public async Task TrySendFiles(ulong messageChannelId, List<FileAttachment> nonConvertableAttachments, string additionalMessage = "Files sent.")
    {
        var textChannel = Client.GetChannel(messageChannelId) as SocketTextChannel;
        if (textChannel is null || !nonConvertableAttachments.Any())
        {
            return;
        }
        
        try
        {
            await textChannel.SendFilesAsync(nonConvertableAttachments, additionalMessage);
        }
        catch (Exception e)
        {
            var errorMessage = "Something went wrong while processing your file(s)..";
            await textChannel.SendMessageAsync(errorMessage);
        }
    }
    
    public async Task<bool> TrySendFile(
        ulong messageChannelId, 
        MemoryStream? fileStream, 
        string uniqueFileName, 
        string extension, 
        string message,
        string additionalMessage = "", 
        string errorMessage = "Something went wrong..")
    {
        var textChannel = Client.GetChannel(messageChannelId) as SocketTextChannel;
        if (textChannel is null)
        {
            return false;
        }
        
        try
        {
            var result = await textChannel.SendFileAsync(fileStream, uniqueFileName + extension, message);
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                await SendMessageFromMessageChannelId(messageChannelId, additionalMessage);
            }
            return true;
        }
        catch (Exception e)
        {
            await textChannel.SendMessageAsync(errorMessage);
        }

        return false;
    }
}