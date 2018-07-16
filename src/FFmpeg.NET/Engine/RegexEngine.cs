using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using FFmpeg.NET.Engine.Models;
using FFmpeg.NET.Events;

namespace FFmpeg.NET.Engine
{
    /// <summary>
    ///     Contains all Regex tasks
    /// </summary>
    internal static class RegexEngine
    {
        /// <summary>
        ///     Dictionary containing every Regex test.
        /// </summary>
        internal static Dictionary<Find, Regex> Index = new Dictionary<Find, Regex>
        {
            {Find.BitRate, new Regex(@"([0-9]*)\s*kb/s")},
            {Find.Duration, new Regex(@"Duration: ([^,]*), ")},
            {Find.ConvertProgressFrame, new Regex(@"frame=\s*([0-9]*)")},
            {Find.ConvertProgressFps, new Regex(@"fps=\s*([0-9]*\.?[0-9]*?)")},
            {Find.ConvertProgressSize, new Regex(@"size=\s*([0-9]*)kB")},
            {Find.ConvertProgressFinished, new Regex(@"Lsize=\s*([0-9]*)kB")},
            {Find.ConvertProgressTime, new Regex(@"time=\s*([^ ]*)")},
            {Find.ConvertProgressBitrate, new Regex(@"bitrate=\s*([0-9]*\.?[0-9]*?)kbits/s")},
            {Find.MetaAudio, new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Audio:.*)")},
            {Find.AudioFormatHzChannel, new Regex(@"Audio:\s*([^,]*),\s([^,]*),\s([^,]*)")},
            {Find.MetaVideo, new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Video:.*)")},
            {Find.VideoFormatColorSize, new Regex(@"Video:\s*([^,]*),\s*([^,]*,?[^,]*?),?\s*(?=[0-9]*x[0-9]*)([0-9]*x[0-9]*)")},
            {Find.VideoFps, new Regex(@"([0-9\.]*)\s*tbr")}
        };

        /// <summary>
        ///     <para> ---- </para>
        ///     <para>Establishes whether the data contains progress information.</para>
        /// </summary>
        /// <param name="data">Event data from the FFmpeg console.</param>
        /// <param name="progressData">
        ///     <para>If successful, outputs a <see cref="ProgressData" /> which is </para>
        ///     <para>generated from the data. </para>
        /// </param>
        internal static bool IsProgressData(string data, out ProgressData progressData)
        {
            progressData = null;

            var matchFrame = Index[Find.ConvertProgressFrame].Match(data);
            var matchFps = Index[Find.ConvertProgressFps].Match(data);
            var matchSize = Index[Find.ConvertProgressSize].Match(data);
            var matchTime = Index[Find.ConvertProgressTime].Match(data);
            var matchBitrate = Index[Find.ConvertProgressBitrate].Match(data);

            if (!matchSize.Success || !matchTime.Success || !matchBitrate.Success)
                return false;

            TimeSpan.TryParse(matchTime.Groups[1].Value, out var processedDuration);

            var frame = GetLongValue(matchFrame);
            var fps = GetDoubleValue(matchFps);
            var sizeKb = GetIntValue(matchSize);
            var bitrate = GetDoubleValue(matchBitrate);

            progressData = new ProgressData(processedDuration, TimeSpan.Zero, frame, fps, sizeKb, bitrate);

            return true;
        }

        private static long? GetLongValue(Match match)
        {
            if (long.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static double? GetDoubleValue(Match match)
        {
            if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static int? GetIntValue(Match match)
        {
            if (int.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        internal static void TestVideo(string data, FFmpegParameters engine)
        {
            var matchMetaVideo = Index[Find.MetaVideo].Match(data);

            if (!matchMetaVideo.Success) return;

            var fullMetadata = matchMetaVideo.Groups[1].ToString();

            var matchVideoFormatColorSize = Index[Find.VideoFormatColorSize].Match(fullMetadata).Groups;
            var matchVideoFps = Index[Find.VideoFps].Match(fullMetadata).Groups;
            var matchVideoBitRate = Index[Find.BitRate].Match(fullMetadata);

            if (engine.InputFile.MetaData == null)
                engine.InputFile.MetaData = new MetaData();

            if (engine.InputFile.MetaData.VideoData == null)
                engine.InputFile.MetaData.VideoData = new MetaData.Video
                {
                    Format = matchVideoFormatColorSize[1].ToString(),
                    ColorModel = matchVideoFormatColorSize[2].ToString(),
                    FrameSize = matchVideoFormatColorSize[3].ToString(),
                    Fps = matchVideoFps[1].Success && !string.IsNullOrEmpty(matchVideoFps[1].ToString()) ? Convert.ToDouble(matchVideoFps[1].ToString(), new CultureInfo("en-US")) : 0,
                    BitRateKbs =
                        matchVideoBitRate.Success
                            ? (int?) Convert.ToInt32(matchVideoBitRate.Groups[1].ToString())
                            : null
                };
        }

        internal static void TestAudio(string data, FFmpegParameters engine)
        {
            var matchMetaAudio = Index[Find.MetaAudio].Match(data);

            if (!matchMetaAudio.Success) return;

            var fullMetadata = matchMetaAudio.Groups[1].ToString();

            var matchAudioFormatHzChannel = Index[Find.AudioFormatHzChannel].Match(fullMetadata).Groups;
            var matchAudioBitRate = Index[Find.BitRate].Match(fullMetadata).Groups;

            if (engine.InputFile.MetaData == null)
                engine.InputFile.MetaData = new MetaData();

            if (engine.InputFile.MetaData.AudioData == null)
                engine.InputFile.MetaData.AudioData = new MetaData.Audio
                {
                    Format = matchAudioFormatHzChannel[1].ToString(),
                    SampleRate = matchAudioFormatHzChannel[2].ToString(),
                    ChannelOutput = matchAudioFormatHzChannel[3].ToString(),
                    BitRateKbs = !string.IsNullOrWhiteSpace(matchAudioBitRate[1].ToString()) ? Convert.ToInt32(matchAudioBitRate[1].ToString()) : 0
                };
        }

        internal enum Find
        {
            AudioFormatHzChannel,
            ConvertProgressBitrate,
            ConvertProgressFps,
            ConvertProgressFrame,
            ConvertProgressSize,
            ConvertProgressFinished,
            ConvertProgressTime,
            Duration,
            MetaAudio,
            MetaVideo,
            BitRate,
            VideoFormatColorSize,
            VideoFps
        }
    }
}