using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace FourCloverConverterBot;

public class FourCloverConverter
{
    //variables
    private DiscordSocketClient _client;
    public CommandService _commands;
    private IServiceProvider _services;
    private DiscordSocketClientHelper _clientHelper;
    private string _defaultConvertTo;
    private string _commandHead;
    private string _botName;
    private string _ffmpegPath;
    
    //log channel info
    private SocketTextChannel LogChannel;

    public async Task Init(string token, string defaultConvertTo, string defaultCommandHead, string botName, string ffmpegPath)
    {
        _defaultConvertTo = defaultConvertTo;
        _commandHead = defaultCommandHead;
        _botName = botName;
        _client = new DiscordSocketClient(new DiscordSocketConfig{GatewayIntents = GatewayIntents.All});
        _clientHelper = new DiscordSocketClientHelper(_client);
        _commands = new CommandService();
        _ffmpegPath = ffmpegPath;

        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();
        
        _client.Log += _client_Log;

        await RegisterCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, token);

        await _client.StartAsync();

        await Task.Delay(-1);
    }
    
    //client log
    private Task _client_Log(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }
    
    //Register Commands Async
    public async Task RegisterCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        
        //console log with message received and user info
        Console.WriteLine("-------------\nUser:  " + message.Author.Username + " with ID  " + message.Author.Id +
                          "\nWrite:" +
                          "\n" + message.ToString());
        
        //return (exit and do nothing) if author of message is the bot
        if (message.Author.IsBot) return;
        
        await CheckForAndCompleteCommands(message);
    }

    private async Task CheckForAndCompleteCommands(SocketUserMessage message)
    {
        if (message.Content.Length >= _commandHead.Length && message.Content.Substring(0, _commandHead.Length).Equals(_commandHead))
        {
            if (CheckForFourCloverConverterCommand(message.ToString(), out var convertTo))
            {
                if (UrlConverterHelper.GetFccUrl(message.Content, out var url) && UrlConverterHelper.UrlIsFccSupported(url, out var convertFrom))
                {
                    await ConvertFromDiscordUrl(message, url, convertTo, convertFrom);
                }
                else
                {
                    await ConvertFromDiscordAttachment(message, convertTo);
                }
            }
            else if (message.Content == $"{_commandHead} help")
            {
                await FourCloverHelpCommand(message);
            }
            else
            {
                await FourCloverInvalidCommandInput(message);
            }
        }
    }

    private async Task FourCloverHelpCommand(SocketUserMessage socketUserMessage)
    {
        await _clientHelper.SendMessageFromMessageChannelId(
            socketUserMessage.Channel.Id,
            MessageHelper.GetHelpMessage(_botName, _commandHead, _defaultConvertTo));
    }
    
    private async Task FourCloverInvalidCommandInput(SocketUserMessage socketUserMessage)
    {
        await _clientHelper.SendMessageFromMessageChannelId(
            socketUserMessage.Channel.Id,
            $@"Issue with processing command, if you need help, please use ""{_commandHead} help"".");
    }

    private bool CheckForFourCloverConverterCommand(string message, out string convertTo)
    {
        message = message.TrimEnd();
        var messageSplit = message.Split(" ");
        var messageHasUrl = UrlConverterHelper.GetFccUrl(message, out _);
        var constructedMessage =
            message == _commandHead ? 
                message + $" {_defaultConvertTo}" : 
                messageSplit.FirstOrDefault() == _commandHead 
                && messageHasUrl
                && !MediaTypes.MediaTypeList.Contains(messageSplit.LastOrDefault() ?? "") ? 
                    message + $" {_defaultConvertTo}" : 
                    message;
        var constructedMessageSplit = constructedMessage.Split(" ");
        var containsConvertToType = MediaTypes.MediaTypeList.Contains(constructedMessageSplit.LastOrDefault() ?? "");
        convertTo = constructedMessageSplit.LastOrDefault() ?? "";
        if ((constructedMessageSplit.Length is 1 or 2 || (messageHasUrl && constructedMessageSplit.Length is 2 or 3))
            && constructedMessageSplit.FirstOrDefault() == _commandHead
            && containsConvertToType)
        {
            return true;
        }

        return false;
    }
    
    private async Task ConvertFromDiscordAttachment(SocketUserMessage message, string convertTo)
    {
        var isDeleted = false;

        ulong messageChannelId = message.Channel.Id;
        var attachments = message.Attachments;

        if (attachments.Count is 0 || !attachments.Any())
        {
            await _clientHelper.SendMessageFromMessageChannelId(messageChannelId, "You need to upload a file.");
            return;
        }
        
        var convertingMessage = await _clientHelper.SendMessageFromMessageChannelId(messageChannelId, "Attempting Conversion(s)...");

        var attachmentUrls =
            attachments
                .Where(x => UrlConverterHelper.CanBuildFfmpegArgsCommand(convertTo,x.Url.Split(".").Last(), x.Url, out _))
                .Select(x => x.Url)
                .ToList();

        var isValidLink = attachmentUrls.All(x => UrlConverterHelper.UrlIsFccSupported(x, out _));

        if (!isValidLink)
        {
            await convertingMessage.DeleteAsync();
            await _clientHelper.SendMessageFromMessageChannelId(messageChannelId, "Invalid File Format(s).");
            return;
        }
        
        using (var httpClient = new HttpClient())
        {
            var nonConvertableAttachments = attachments
                    .Where(x => !UrlConverterHelper.CanBuildFfmpegArgsCommand(convertTo,x.Url.Split(".").Last(), x.Url, out _))
                    .Select(async x => new FileAttachment(await httpClient.GetStreamAsync(x.Url), x.Filename))
                    .ToList();
        
            var nonConvertableFiles = nonConvertableAttachments.Any() 
                ? (await Task.WhenAll(nonConvertableAttachments)).ToList()
                : new List<FileAttachment>();

            await _clientHelper.TrySendFiles(messageChannelId, nonConvertableFiles);
        }

        foreach (var attachmentUrl in attachmentUrls)
        {
            UrlConverterHelper.UrlIsFccSupported(attachmentUrl, out var convertFrom);
            var fileStream = await UrlConverterHelper.ConvertAsync(attachmentUrl, convertTo, convertFrom, _ffmpegPath);
            if (fileStream == null)
            {
                var textChannel = _client.GetChannel(messageChannelId) as SocketTextChannel;
                await textChannel.SendMessageAsync("File conversion mismatch");
                return;
            }
            var uniqueFileName = UrlConverterHelper.GenerateRandomGuidString();

            var fileSent = await _clientHelper.TrySendFile(
                messageChannelId, 
                fileStream, 
                uniqueFileName, 
                $".{convertTo}", 
                $"Meme converted from {convertFrom} to {convertTo}", 
                "",
                "File too big dood...");

            if (fileSent && convertingMessage is not null && !isDeleted)
            {
                await message.DeleteAsync();
                await convertingMessage.DeleteAsync();
                isDeleted = true;
            }
        }
    }

    private async Task ConvertFromDiscordUrl(SocketUserMessage message, string url, string convertTo, string convertFrom)
    {
        ulong messageChannelId = message.Channel.Id;
        
        var convertingMessage = await _clientHelper.SendMessageFromMessageChannelId(messageChannelId, "Attempting Conversion...");
        
        var fileStream = await UrlConverterHelper.ConvertAsync(url, convertTo, convertFrom, _ffmpegPath);
        if (fileStream == null)
        {
            var textChannel = _client.GetChannel(messageChannelId) as SocketTextChannel;
            await textChannel.SendMessageAsync("File conversion mismatch");
            return;
        }
        var uniqueFileName = UrlConverterHelper.GenerateRandomGuidString();

        var fileSent = await _clientHelper.TrySendFile(
            messageChannelId, 
            fileStream, 
            uniqueFileName, 
            $".{convertTo}", 
            $"Meme converted from {convertFrom} to {convertTo}", 
            "Here is your shit dood.",
            "File too big dood...");

        if (fileSent && convertingMessage is not null)
        {
            await message.DeleteAsync();
            await convertingMessage.DeleteAsync();
        }
    }
}