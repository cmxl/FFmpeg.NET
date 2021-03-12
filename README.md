[<img src="lib/ffmpeg/v4/icon.png" alt="drawing" width="24" height="24" /> FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET)
============

[FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) provides a straightforward interface for handling media data, making tasks such as converting, slicing and editing both audio and video completely effortless.

Under the hood, [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) is a .NET wrapper for FFmpeg; a free (LGPLv2.1) multimedia framework containing multiple audio and video codecs, supporting muxing, demuxing and transcoding tasks on many media formats.

Some major parts are taken from https://github.com/AydinAdn/MediaToolkit.
Many features have been refactored. The library has been ported to Netstandard and made threadsafe.

You need to provide the ffmpeg executable path to the `Engine` constructor.

## Project Health

| Service | Status |
| --- | --- |
| Travis CI | [![Build Status](https://travis-ci.com/cmxl/FFmpeg.NET.svg?branch=master)](https://travis-ci.com/cmxl/FFmpeg.NET) |

## Packages

| Package | NuGet |
| --- | --- |
| xFFmpeg.NET | [![NuGet](https://buildstats.info/nuget/xFFmpeg.NET)](https://www.nuget.org/packages/xFFmpeg.NET) |

## Contents

1. [Features](#features)
2. [Get started!](#get-started)
3. [Samples](#samples)
4. [Licensing](#licensing)

## Features

- Resolving metadata
- Generating thumbnails from videos
- Transcode audio & video into other formats using parameters such as:
    -  `Bit rate`
    -  `Frame rate`
    -  `Resolution`
    -  `Aspect ratio`
    -  `Seek position`
    -  `Duration`
    -  `Sample rate`
    -  `Media format`
- Convert media to physical formats and standards such as:
    - Standards include: `FILM`, `PAL` & `NTSC`
    - Mediums include: `DVD`, `DV`, `DV50`, `VCD` & `SVCD`
- Supports custom FFmpeg command line arguments (*NEW in v2.1.0*)
- Raising progress events

## Get started!

Install [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) from nuget.org Package Source using the Package Manager Console with the following command

    PM> Install-Package xFFmpeg.NET

## Samples

- [Grab thumbnail from a video](#grab-thumbnail-from-a-video)
- [Retrieve metadata](#retrieve-metadata)  new Engine
- [Perform basic video conversions](#basic-conversion)  
- [Convert from FLV to DVD](#convert-flash-video-to-dvd)  
- [Convert FLV to MP4 using various transcoding options](#transcoding-options-flv-to-mp4)  
- [Cut / split video](#cut-video-down-to-smaller-length)
- [Subscribing to events](#subscribe-to-events)

### Grab thumbnail from a video

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
var outputFile = new MediaFile (@"C:\Path\To_Save_Image.jpg");

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
// Saves the frame located on the 15th second of the video.
var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(15) };
await ffmpeg.GetThumbnailAsync(inputFile, outputFile, options);
```

### Retrieve metadata

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
var metadata = await ffmpeg.GetMetadataAsync(inputFile);

Console.WriteLine(metadata.Duration);
```

### Basic conversion

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
var outputFile = new MediaFile (@"C:\Path\To_Save_New_Video.mp4");

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
await ffmpeg.ConvertAsync(inputFile, outputFile);
```

### Convert Flash video to DVD

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
var outputFile = new MediaFile (@"C:\Path\To_Save_New_DVD.vob");

var conversionOptions = new ConversionOptions
{
    Target = Target.DVD,
    TargetStandard = TargetStandard.PAL
};

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions);
```

### Transcoding options FLV to MP4

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
var outputFile = new MediaFile (@"C:\Path\To_Save_New_Video.mp4");

var conversionOptions = new ConversionOptions
{
    MaxVideoDuration = TimeSpan.FromSeconds(30),
    VideoAspectRatio = VideoAspectRatio.R16_9,
    VideoSize = VideoSize.Hd1080,
    AudioSampleRate = AudioSampleRate.Hz44100
};

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions);
```

### Cut video down to smaller length

```csharp
var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
var outputFile = new MediaFile (@"C:\Path\To_Save_ExtractedVideo.flv");

var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
var options = new ConversionOptions();

// This example will create a 25 second video, starting from the 
// 30th second of the original video.
//// First parameter requests the starting frame to cut the media from.
//// Second parameter requests how long to cut the video.
options.CutMedia(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(25));
await ffmpeg.ConvertAsync(inputFile, outputFile, options);
```

### Subscribe to events

```csharp
public async Task StartConverting()
{
    var inputFile = new MediaFile (@"C:\Path\To_Video.flv");
    var outputFile = new MediaFile (@"C:\Path\To_Save_New_Video.mp4");

    var ffmpeg = new Engine("C:\\ffmpeg\\ffmpeg.exe");
    ffmpeg.Progress += OnProgress;
    ffmpeg.Data += OnData;
    ffmpeg.Error += OnError;
    ffmpeg.Complete += OnComplete;
    await ffmpeg.ConvertAsync(inputFile, outputFile);
}

private void OnProgress(object sender, ConversionProgressEventArgs e)
{
    Console.WriteLine("[{0} => {1}]", e.Input.FileInfo.Name, e.Output.FileInfo.Name);
    Console.WriteLine("Bitrate: {0}", e.Bitrate);
    Console.WriteLine("Fps: {0}", e.Fps);
    Console.WriteLine("Frame: {0}", e.Frame);
    Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
    Console.WriteLine("Size: {0} kb", e.SizeKb);
    Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
}

private void OnData(object sender, ConversionDataEventArgs e)
{
    Console.WriteLine("[{0} => {1}]: {2}", e.Input.FileInfo.Name, e.Output.FileInfo.Name, e.Data);
}

private void OnComplete(object sender, ConversionCompleteEventArgs e)
{
    Console.WriteLine("Completed conversion from {0} to {1}", e.Input.FileInfo.FullName, e.Output.FileInfo.FullName);
}

private void OnError(object sender, ConversionErrorEventArgs e)
{
    Console.WriteLine("[{0} => {1}]: Error: {2}\n{3}", e.Input.FileInfo.Name, e.Output.FileInfo.Name, e.Exception.ExitCode, e.Exception.InnerException);
}
```

### Licensing
---------  
- Forwards licensing of [MediaToolkit](https://github.com/AydinAdn/MediaToolkit/blob/master/LICENSE.md)
- [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) is licensed under the [MIT license](https://github.com/cmxl/FFmpeg.NET/blob/master/LICENSE.md)
- [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) uses [FFmpeg](http://ffmpeg.org), a multimedia framework which is licensed under the [LGPLv2.1 license](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html)
