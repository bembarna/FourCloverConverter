// See https://aka.ms/new-console-template for more information

using FourCloverConverterBot;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var discordToken = config["DiscordToken"];
var defaultConvertToExtension = config["DefaultConvertToExtension"];
var defaultCommandHead = config["DefaultCommandHead"];
var botName = config["BotName"];
var ffmpegPath = config["FfmpegPath"];

new FourCloverConverter().Init(discordToken, defaultConvertToExtension, defaultCommandHead, botName, ffmpegPath).GetAwaiter().GetResult();