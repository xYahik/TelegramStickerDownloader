using System.IO.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using SkiaSharp.Skottie;
public class TgsConverter
{
    public static void ConvertTgsToGif(string tgsPath, string gifPath)
    {
        string json;
        using (var fileStream = File.OpenRead(tgsPath))
        using (var gzip = new GZipStream(fileStream, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzip))
        {
            json = reader.ReadToEnd();
        }

        var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        //SkiaSharp doesn't accept string as path, so json need to be processed as Stream
        var animation = Animation.Create(memoryStream);
        if (animation == null)
        {
            return;
        }

        int width = (int)animation.Size.Width;
        int height = (int)animation.Size.Height;
        int frames = (int)(animation.Duration.TotalSeconds * animation.Fps);

        var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        gifMetadata.BackgroundColorIndex = 0;

        for (int currentFrame = 0; currentFrame < frames; currentFrame++)
        {
            animation.SeekFrame(currentFrame);
            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            animation.Render(canvas, new SKRect(0, 0, width, height));

            var frameMemoryStream = new MemoryStream();
            bitmap.Encode(frameMemoryStream, SKEncodedImageFormat.Png, 100);
            frameMemoryStream.Seek(0, SeekOrigin.Begin);

            var frame = Image.Load<Rgba32>(frameMemoryStream);

            gif.Frames.AddFrame(frame.Frames.RootFrame);

            //setup frame parameters
            var frameMeta = gif.Frames[^1].Metadata.GetGifMetadata();
            frameMeta.FrameDelay = 2;
            frameMeta.DisposalMethod = GifDisposalMethod.RestoreToBackground;

        }

        //Deleting empty frame at start, which was created with gif
        gif.Frames.RemoveFrame(0);

        gif.SaveAsGif(gifPath);
    }
}