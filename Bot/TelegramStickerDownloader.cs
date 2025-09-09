using System.IO.Compression;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot;

public class TelegramStickerDownloader
{
    private static TelegramBotClient _bot;
    private static string _downloadFolderName = "DownloadedStickers";
    public async static Task Initialize()
    {
        _bot = new TelegramBotClient(CONFIG.Token);

        if (!Directory.Exists(_downloadFolderName))
        {
            Directory.CreateDirectory(_downloadFolderName);
        }
        while (true)
        {
            Console.WriteLine("Paste url to sticker pack (eg. https://t.me/addstickers/UtyaDuck):");
            string url = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Url is invalid!");
                continue;
            }

            var match = Regex.Match(url, @"(t(elegram)?\.me)\/addstickers\/([A-Za-z0-9_]+)");
            if (match.Success)
            {
                string stickerNameSet = match.Groups[3].Value;
                await GetStickers(stickerNameSet);
            }
            else
            {
                Console.WriteLine("Couldn't match url correctly, make sure that your url is correct");
            }
        }
    }

    public async static Task GetStickers(string stickerNameSet)
    {
        var stickerSet = await _bot.GetStickerSet(stickerNameSet);
        Console.WriteLine($"Found {stickerSet.Stickers.Count()} stickers in pack {stickerSet.Name}");

        using var httpClient = new HttpClient();

        int stickerProgress = 0;
        int barLength = 30;

        FileStream? zipWebpStream = null;
        ZipArchive? zipWebp = null;
        FileStream? zipWebmStream = null;
        ZipArchive? zipWebm = null;
        FileStream? zipPngStream = null;
        ZipArchive? zipPng = null;
        FileStream? zipGifStream = null;
        ZipArchive? zipGif = null;

        foreach (var sticker in stickerSet.Stickers)
        {
            var file = await _bot.GetFile(sticker.FileId);
            string url = $"https://api.telegram.org/file/bot{CONFIG.Token}/{file.FilePath}";
            byte[] data = await httpClient.GetByteArrayAsync(url);

            string ext = sticker.IsAnimated ? ".tgs" : ".webp";

            if (sticker.IsAnimated)
            {

                if (zipGif == null)
                {
                    zipGifStream = new FileStream(Path.Combine(_downloadFolderName, $"{stickerNameSet}_gif.zip"), FileMode.Create);
                    zipGif = new ZipArchive(zipGifStream, ZipArchiveMode.Create);
                }

                string gifName = $"{stickerNameSet}_{sticker.FileUniqueId}.gif";
                string tempTgs = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempTgs, data);
                string tempGif = Path.ChangeExtension(tempTgs, ".gif");
                TgsConverter.ConvertTgsToGif(tempTgs, tempGif);
                zipGif.CreateEntryFromFile(tempGif, gifName);



                stickerProgress++;
            }
            //Extra step to check if file is WEBM, by some reasons, telegram accept animation as WEBM but not set it as animated sticker
            else if (string.Equals(Path.GetExtension(file.FilePath), ".webm", StringComparison.OrdinalIgnoreCase))
            {
                //WEBM
                if (zipWebm == null)
                {
                    zipWebmStream = new FileStream(Path.Combine(_downloadFolderName, $"{stickerNameSet}_webm.zip"), FileMode.Create);
                    zipWebm = new ZipArchive(zipWebmStream, ZipArchiveMode.Create);
                }
                string webmName = $"{stickerNameSet}_{sticker.FileUniqueId}.webm";
                var entryWebm = zipWebm.CreateEntry(webmName);
                using (var entryStream = entryWebm.Open())
                {
                    await entryStream.WriteAsync(data, 0, data.Length);
                }


                stickerProgress++;
            }
            else
            {

                //WEBP
                if (zipWebp == null)
                {
                    zipWebpStream = new FileStream(Path.Combine(_downloadFolderName, $"{stickerNameSet}_webp.zip"), FileMode.Create);
                    zipWebp = new ZipArchive(zipWebpStream, ZipArchiveMode.Create);
                }
                string webpName = $"{stickerNameSet}_{sticker.FileUniqueId}.webp";
                var entryWebp = zipWebp.CreateEntry(webpName);
                using (var entryStream = entryWebp.Open())
                {
                    await entryStream.WriteAsync(data, 0, data.Length);
                }


                //PNG
                if (zipPng == null)
                {
                    zipPngStream = new FileStream(Path.Combine(_downloadFolderName, $"{stickerNameSet}_png.zip"), FileMode.Create);
                    zipPng = new ZipArchive(zipPngStream, ZipArchiveMode.Create);
                }
                using var img = Image.Load<Rgba32>(data);
                string pngName = $"{stickerNameSet}_{sticker.FileUniqueId}.png";
                var entryPng = zipPng.CreateEntry(pngName);
                using (var entryStream = entryPng.Open())
                {
                    await img.SaveAsPngAsync(entryStream, new PngEncoder());
                }


                stickerProgress++;
            }

            float progressPercent = (float)stickerProgress / stickerSet.Stickers.Count();
            int filled = (int)(progressPercent * barLength);

            Console.ForegroundColor = GetConsoleProgressionColor(progressPercent);

            string barText = $"[{new string('=', filled)}{new string(' ', barLength - filled)}]  {stickerProgress}/{stickerSet.Stickers.Count()} Stickers";
            Console.Write($"\r {barText}");
        }

        zipWebp?.Dispose();
        zipWebpStream?.Dispose();
        zipWebm?.Dispose();
        zipWebmStream?.Dispose();
        zipPng?.Dispose();
        zipPngStream?.Dispose();
        zipGif?.Dispose();
        zipGifStream?.Dispose();

        Console.WriteLine("\n Completed");
        Console.ResetColor();
    }

    public static ConsoleColor GetConsoleProgressionColor(float progressPercent)
    {
        if (progressPercent < 0.33)
            return ConsoleColor.Red;
        else if (progressPercent < 0.66)
            return ConsoleColor.DarkYellow;
        else if (progressPercent < 1.0)
            return ConsoleColor.Yellow;
        else
            return ConsoleColor.Green;
    }
}