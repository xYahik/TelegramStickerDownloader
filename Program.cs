// See https://aka.ms/new-console-template for more information
using System.IO.Compression;
using System.Security;
using Telegram.Bot;

class Program
{
    static async Task Main()
    {
        await TelegramStickerDownloader.Initialize();

    }
}