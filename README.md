[<img src="lib/ffmpeg/v4/icon.png" alt="drawing" width="24" height="24" /> FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET)
============

[FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) provides a straightforward interface for handling media data, making tasks such as converting, slicing and editing both audio and video completely effortless.

Under the hood, [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) is a .NET wrapper for FFmpeg; a free (LGPLv2.1) multimedia framework containing multiple audio and video codecs, supporting muxing, demuxing and transcoding tasks on many media formats.

The library targets **netstandard2.1**, is fully async, thread-safe, and works cross-platform (Windows, Linux, macOS).

## Packages

| Package                                                   | NuGet                                                                                                                                                                                                                        |
|-----------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [xFFmpeg.NET](https://www.nuget.org/packages/xFFmpeg.NET) | [![Package Version](https://img.shields.io/nuget/v/xFFmpeg.NET)](https://www.nuget.org/packages/xFFmpeg.NET) [![Package Downloads](https://img.shields.io/nuget/dt/xFFmpeg.NET)](https://www.nuget.org/packages/xFFmpeg.NET) |

## Contents

1. [Features](#features)
2. [Get started](#get-started)
3. [Samples](#samples)
4. [Licensing](#licensing)

## Features

- Transcode audio & video into other formats using parameters such as:
    -  `Bit rate`
    -  `Frame rate`
    -  `Resolution` (predefined sizes or custom width/height)
    -  `Aspect ratio`
    -  `Seek position`
    -  `Duration`
    -  `Sample rate`
    -  `Media format`
    -  `Codec presets & profiles` (ultrafast to veryslow, baseline to high444)
    -  `Pixel format`
- Resolving metadata (duration, codecs, resolution, bitrate, fps)
- Generating thumbnails from videos
- Hardware-accelerated encoding/decoding (CUDA, QSV, DXVA2, D3D11VA)
- Stream and pipe-based I/O (no intermediate files required)
- Video cropping, speed control, and audio removal
- Custom FFmpeg command line arguments via `ExecuteAsync`
- Full `CancellationToken` support on all async operations
- Progress, error, completion, and raw data events
- Playlist generation (M3U, XSPF)
- Convert media to physical formats and standards such as:
    - Standards include: `FILM`, `PAL` & `NTSC`
    - Mediums include: `DVD`, `DV`, `DV50`, `VCD` & `SVCD`

## Get started

You need to provide the FFmpeg executable path to the `Engine` constructor, or ensure `ffmpeg` is available in your system PATH.

```bash
dotnet add package xFFmpeg.NET
```

Or via the Package Manager Console:

    PM> Install-Package xFFmpeg.NET

## Samples

- [Grab thumbnail from a video](#grab-thumbnail-from-a-video)
- [Retrieve metadata](#retrieve-metadata)
- [Basic conversion](#basic-conversion)
- [H.264 encoding with presets and profiles](#h264-encoding-with-presets-and-profiles)
- [Extract audio from video](#extract-audio-from-video)
- [Hardware-accelerated conversion](#hardware-accelerated-conversion)
- [Cut / split video](#cut-video-down-to-smaller-length)
- [Crop video](#crop-video)
- [Change video speed](#change-video-speed)
- [Convert using streams](#convert-using-streams)
- [Custom FFmpeg arguments](#custom-ffmpeg-arguments)
- [CancellationToken support](#cancellationtoken-support)
- [Subscribe to events](#subscribe-to-events)

### Grab thumbnail from a video

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("thumbnail.jpg");

var ffmpeg = new Engine("/usr/bin/ffmpeg");
// Saves the frame located on the 15th second of the video.
var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(15) };
await ffmpeg.GetThumbnailAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Retrieve metadata

```csharp
var inputFile = new InputFile("video.mp4");

var ffmpeg = new Engine("/usr/bin/ffmpeg");
var metadata = await ffmpeg.GetMetaDataAsync(inputFile, CancellationToken.None);

Console.WriteLine($"Duration: {metadata.Duration}");
Console.WriteLine($"Video: {metadata.VideoData?.Format} {metadata.VideoData?.FrameSize} @ {metadata.VideoData?.Fps} fps");
Console.WriteLine($"Audio: {metadata.AudioData?.Format} {metadata.AudioData?.SampleRate} {metadata.AudioData?.ChannelOutput}");
```

### Basic conversion

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video.mkv");

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, CancellationToken.None);
```

### H.264 encoding with presets and profiles

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video_h264.mp4");

var options = new ConversionOptions
{
    VideoCodec = VideoCodec.libx264,
    VideoCodecPreset = VideoCodecPreset.fast,
    VideoCodecProfile = VideoCodecProfile.main,
    VideoBitRate = 4000,
    VideoFps = 30,
    VideoSize = VideoSize.Hd1080,
    VideoAspectRatio = VideoAspectRatio.R16_9,
    AudioSampleRate = AudioSampleRate.Hz48000,
    AudioBitRate = 192,
    ExtraArguments = "-movflags +faststart"
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Extract audio from video

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("audio.mp3");

var options = new ConversionOptions
{
    AudioBitRate = 320,
    AudioSampleRate = AudioSampleRate.Hz44100
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Hardware-accelerated conversion

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video_gpu.mp4");

// NVIDIA CUDA acceleration
var options = new ConversionOptions
{
    HWAccel = HWAccel.cuda,
    VideoCodec = VideoCodec.h264_nvenc,
    VideoBitRate = 5000,
    VideoSize = VideoSize.Hd1080
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Cut video down to smaller length

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("clip.mp4");

var options = new ConversionOptions();

// Creates a 25 second clip, starting from the 30th second of the original video.
options.CutMedia(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(25));

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Crop video

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video_cropped.mp4");

var options = new ConversionOptions
{
    SourceCrop = new CropRectangle { X = 100, Y = 50, Width = 1280, Height = 720 }
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Change video speed

```csharp
var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video_fast.mp4");

var options = new ConversionOptions
{
    // 0.5 = 2x speed, 2.0 = half speed
    VideoTimeScale = 0.5
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
```

### Convert using streams

```csharp
// Convert to a MemoryStream (no output file needed)
var inputFile = new InputFile("video.mp4");
var options = new ConversionOptions
{
    VideoCodec = VideoCodec.libx264,
    VideoSize = VideoSize.Hd720
};

var ffmpeg = new Engine("/usr/bin/ffmpeg");
using var outputStream = await ffmpeg.ConvertAsync(inputFile, options, CancellationToken.None);

// Or pipe a .NET Stream into FFmpeg as input
await using var fileStream = File.OpenRead("video.mp4");
await using var streamInput = new StreamInput(fileStream);

var output = new OutputFile("converted.mkv");
await ffmpeg.ConvertAsync(streamInput, output, CancellationToken.None);
```

### Custom FFmpeg arguments

```csharp
var ffmpeg = new Engine("/usr/bin/ffmpeg");

// Run any FFmpeg command directly
await ffmpeg.ExecuteAsync("-i input.mp4 -vn -acodec libmp3lame -q:a 2 output.mp3", CancellationToken.None);

// With a working directory
await ffmpeg.ExecuteAsync("-i input.mp4 -c copy output.mkv", "/path/to/media", CancellationToken.None);
```

### CancellationToken support

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

var inputFile = new InputFile("video.mp4");
var outputFile = new OutputFile("video.mkv");

var ffmpeg = new Engine("/usr/bin/ffmpeg");

try
{
    await ffmpeg.ConvertAsync(inputFile, outputFile, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Conversion was cancelled or timed out.");
}
```

### Subscribe to events

```csharp
public async Task StartConverting()
{
    var inputFile = new InputFile("video.mp4");
    var outputFile = new OutputFile("video.mkv");

    var ffmpeg = new Engine("/usr/bin/ffmpeg");
    ffmpeg.Progress += OnProgress;
    ffmpeg.Data += OnData;
    ffmpeg.Error += OnError;
    ffmpeg.Complete += OnComplete;
    await ffmpeg.ConvertAsync(inputFile, outputFile, CancellationToken.None);
}

private void OnProgress(object sender, ConversionProgressEventArgs e)
{
    Console.WriteLine("[{0} => {1}]", e.Input.Name, e.Output.Name);
    Console.WriteLine("Bitrate: {0}", e.Bitrate);
    Console.WriteLine("Fps: {0}", e.Fps);
    Console.WriteLine("Frame: {0}", e.Frame);
    Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
    Console.WriteLine("Size: {0} kb", e.SizeKb);
    Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
}

private void OnData(object sender, ConversionDataEventArgs e)
{
    Console.WriteLine("[{0} => {1}]: {2}", e.Input.Name, e.Output.Name, e.Data);
}

private void OnComplete(object sender, ConversionCompleteEventArgs e)
{
    Console.WriteLine("Completed conversion from {0} to {1}", e.Input.Name, e.Output.Name);
}

private void OnError(object sender, ConversionErrorEventArgs e)
{
    Console.WriteLine("[{0} => {1}]: Error: {2}\n{3}", e.Input.Name, e.Output.Name, e.Exception.ExitCode, e.Exception.InnerException);
}
```

---------

### Licensing
- [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) is licensed under the [MIT license](https://github.com/cmxl/FFmpeg.NET/blob/master/LICENSE.md)
- [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) uses [FFmpeg](http://ffmpeg.org), a multimedia framework which is licensed under the [LGPLv2.1 license](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html)
- Originally based on [MediaToolkit](https://github.com/AydinAdn/MediaToolkit) ([license](https://github.com/AydinAdn/MediaToolkit/blob/master/LICENSE.md)), substantially refactored and ported to netstandard2.1
