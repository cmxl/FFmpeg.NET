using FFmpeg.NET.Enums;
using System;
using System.Globalization;
using System.Text;

namespace FFmpeg.NET
{
    internal static class FFmpegArgumentBuilder
    {
        public static string Build(FFmpegParameters parameters)
        {
            if (parameters.HasCustomArguments)
                return parameters.CustomArguments;

            switch (parameters.Task)
            {
                case FFmpegTask.Convert:
                    return Convert(parameters.InputFile, parameters.OutputFile, parameters.ConversionOptions);

                case FFmpegTask.GetMetaData:
                    return GetMetadata(parameters.InputFile);

                case FFmpegTask.GetThumbnail:
                    return GetThumbnail(parameters.InputFile, parameters.OutputFile, parameters.ConversionOptions);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetMetadata(MediaFile inputFile) => $"-i \"{inputFile.FileInfo.FullName}\" -f ffmetadata -";

        private static string GetThumbnail(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var defaultTimeSpan = TimeSpan.FromSeconds(1);
            var commandBuilder = new StringBuilder();

            commandBuilder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ", conversionOptions?.Seek.GetValueOrDefault(defaultTimeSpan).TotalSeconds ?? defaultTimeSpan.TotalSeconds);

            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.FileInfo.FullName);
            commandBuilder.AppendFormat(" -vframes {0} ", 1);

            // Video size / resolution
            commandBuilder = AppendVideoSize(commandBuilder, conversionOptions);

            // Video aspect ratio
            commandBuilder = AppendVideoAspectRatio(commandBuilder, conversionOptions);

            // Video cropping
            commandBuilder = AppendVideoCropping(commandBuilder, conversionOptions);

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.FileInfo.FullName).ToString();
        }

        private static string Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var commandBuilder = new StringBuilder();

            // Default conversion
            if (conversionOptions == null)
                return commandBuilder.AppendFormat(" -i \"{0}\" \"{1}\" ", inputFile.FileInfo.FullName, outputFile.FileInfo.FullName).ToString();

            if (conversionOptions.HideBanner) commandBuilder.Append(" -hide_banner ");

            if (conversionOptions.Threads != 0)
            {
                commandBuilder.AppendFormat(" -threads {0} ", conversionOptions.Threads);
            }

            // HW Accel
            if (conversionOptions.HWAccel != HWAccel.None)
            {
                commandBuilder.AppendFormat(" -hwaccel {0} ", conversionOptions.HWAccel);
                AppendHWAccelOutputFormat(commandBuilder, conversionOptions);
            }

            // Media seek position
            if (conversionOptions.Seek != null)
                commandBuilder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ", conversionOptions.Seek.Value.TotalSeconds);

            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.FileInfo.FullName);

            // Physical media conversion (DVD etc)
            if (conversionOptions.Target != Target.Default)
            {
                commandBuilder.Append(" -target ");
                if (conversionOptions.TargetStandard != TargetStandard.Default)
                {
                    commandBuilder.AppendFormat(" {0}-{1} \"{2}\" ", conversionOptions.TargetStandard.ToString().ToLowerInvariant(), conversionOptions.Target.ToString().ToLowerInvariant(), outputFile.FileInfo.FullName);

                    return commandBuilder.ToString();
                }

                commandBuilder.AppendFormat("{0} \"{1}\" ", conversionOptions.Target.ToString().ToLowerInvariant(), outputFile.FileInfo.FullName);

                return commandBuilder.ToString();
            }

            // Video Format
            commandBuilder = AppendVideoFormat(commandBuilder, conversionOptions);

            // Video Codec
            commandBuilder = AppendVideoCodec(commandBuilder, conversionOptions);

            // Video Codec Preset
            if (conversionOptions.VideoCodecPreset != VideoCodecPreset.Default)
                commandBuilder.AppendFormat(" -preset {0} ", conversionOptions.VideoCodecPreset);

            // Video Codec Profile
            if (conversionOptions.VideoCodecProfile != VideoCodecProfile.Default)
                commandBuilder.AppendFormat(" -profile:v {0} ", conversionOptions.VideoCodecProfile);

            // Video Time Scale
            if (conversionOptions.VideoTimeScale != null && conversionOptions.VideoTimeScale != 1)
                commandBuilder.AppendFormat(" -filter:v \"setpts = {0} * PTS\" ", conversionOptions.VideoTimeScale.ToString().Replace(",","."));

            // Maximum video duration
            if (conversionOptions.MaxVideoDuration != null)
                commandBuilder.AppendFormat(" -t {0} ", conversionOptions.MaxVideoDuration);

            // Video bit rate
            if (conversionOptions.VideoBitRate != null)
                commandBuilder.AppendFormat(" -b:v {0}k ", conversionOptions.VideoBitRate);

            // Video frame rate
            if (conversionOptions.VideoFps != null)
                commandBuilder.AppendFormat(" -r {0} ", conversionOptions.VideoFps);

            // Video pixel format
            if (conversionOptions.PixelFormat != null)
                commandBuilder.AppendFormat(" -pix_fmt {0} ", conversionOptions.PixelFormat);

            // Video size / resolution
            commandBuilder = AppendVideoSize(commandBuilder, conversionOptions);

            // Video aspect ratio
            commandBuilder = AppendVideoAspectRatio(commandBuilder, conversionOptions);

            // Video cropping
            commandBuilder = AppendVideoCropping(commandBuilder, conversionOptions);
            
            #region Audio
            // Audio bit rate
            if (conversionOptions.AudioBitRate != null)
                commandBuilder.AppendFormat(" -ab {0}k", conversionOptions.AudioBitRate);

            // Audio sample rate
            if (conversionOptions.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", conversionOptions.AudioSampleRate.ToString().Replace("Hz", ""));

            // AudioChannel
            if (conversionOptions.AudioChanel != null)
                commandBuilder.AppendFormat(" -ac {0} ", conversionOptions.AudioChanel);

            // Remove Audio
            if (conversionOptions.RemoveAudio)
                commandBuilder.Append(" -an ");
            #endregion

            if (conversionOptions.MapMetadata) commandBuilder.Append(" -map_metadata 0 ");

            // Extra arguments
            if (conversionOptions.ExtraArguments != null)
                commandBuilder.AppendFormat(" {0} ", conversionOptions.ExtraArguments);

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.FileInfo.FullName).ToString();
        }

        private static void AppendHWAccelOutputFormat(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if(conversionOptions.HWAccel != HWAccel.None && conversionOptions.HWAccelOutputFormatCopy)
            {
                HWAccel accel = conversionOptions.HWAccel;
                bool add = false;
                switch (conversionOptions.HWAccel)
                {
                    case HWAccel.cuda:
                    case HWAccel.cuvid:
                        add = true;
                        accel = HWAccel.cuda;
                        break;
                    case HWAccel.dxva2:     //Not tested
                    case HWAccel.qsv:       //Not tested
                    case HWAccel.d3d11va:   //Not tested
                    default:
                        break;
                }
                if(add) commandBuilder.AppendFormat(" -hwaccel_output_format {0} ", accel);
            }
        }

        private static StringBuilder AppendVideoCropping(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if (conversionOptions.SourceCrop != null)
            {
                var crop = conversionOptions.SourceCrop;
                commandBuilder.AppendFormat(" -filter:v \"crop={0}:{1}:{2}:{3}\" ", crop.Width, crop.Height, crop.X, crop.Y);
            }
            return commandBuilder;
        }

        private static StringBuilder AppendVideoAspectRatio(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if (conversionOptions.VideoAspectRatio != VideoAspectRatio.Default)
            {
                var ratio = conversionOptions.VideoAspectRatio.ToString();
                ratio = ratio.Substring(1);
                ratio = ratio.Replace("_", ":");

                commandBuilder.AppendFormat(" -aspect {0} ", ratio);
            }
            return commandBuilder;
        }

        private static StringBuilder AppendVideoSize(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if (conversionOptions.VideoSize == VideoSize.Custom)
            {
                commandBuilder.AppendFormat(" -vf \"scale={0}:{1}\" ", conversionOptions.CustomWidth ?? -2, conversionOptions.CustomHeight ?? -2);
            }
            else if (conversionOptions.VideoSize != VideoSize.Default)
            {
                var size = conversionOptions.VideoSize.ToString().ToLowerInvariant();
                if (size.StartsWith("_"))
                    size = size.Replace("_", "");
                if (size.Contains("_"))
                    size = size.Replace("_", "-");

                commandBuilder.AppendFormat(" -s {0} ", size);
            }
            return commandBuilder;
        }


        private static StringBuilder AppendVideoCodec(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if (conversionOptions.VideoCodec != VideoCodec.Default)
            {
                var codec = conversionOptions.VideoCodec.ToString().ToLowerInvariant();
                commandBuilder.AppendFormat(" -vcodec {0} ", codec);
            }
            return commandBuilder;
        }

        private static StringBuilder AppendVideoFormat(StringBuilder commandBuilder, ConversionOptions conversionOptions)
        {
            if (conversionOptions.VideoFormat != VideoFormat.Default)
            {
                var format = conversionOptions.VideoFormat.ToString().ToLowerInvariant();
                if (format.StartsWith("_"))
                    format = format.Replace("_", "");

                commandBuilder.AppendFormat(" -f {0} ", format);
            }
            return commandBuilder;
        }
    }
}
