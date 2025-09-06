# TelegramStickerDownloader

TelegramStickerDownloader is a simple console .NET project that allows you to download stickers from TelegramApp.

![ShowcaseImage](https://i.imgur.com/rPVBuOd.png)

**Currently Supported Formats:**

- Static Stickers:
  - WEBP
  - PNG
- Animated Stickers:
  - GIF

## Technologies

- C# (.NET)
- Telegram API

## Requirements

- Telegram Bot Token - [Tutorial how create your own bot](https://core.telegram.org/bots/tutorial#obtain-your-bot-token)

## Getting Started

1.**Ensure you have the required software installed**

2.**Configure**:  
 Edit `Bot/Config.cs` to set your [TelegramBot token](https://core.telegram.org/bots/tutorial#obtain-your-bot-token):

```csharp
public static class CONFIG
{
    public static string Token => "<YOUR-TELEGRAM-BOT-TOKEN>";
}
```

3.**Run the program:**

```bash
  dotnet run
```

## License

This project is licensed under the **Apache License 2.0**.  
You are free to use, modify, and distribute the code.  
If you modify the code, please keep a note about the original author (me) to give proper credit.

For more details, see the [LICENSE](LICENSE) file.
