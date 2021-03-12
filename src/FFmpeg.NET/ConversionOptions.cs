using System;
using FFmpeg.NET.Enums;

namespace FFmpeg.NET
{
    public class ConversionOptions
    {

        /// <summary>
        ///     Hide Banner
        /// </summary>
        public bool HideBanner { get; set; } = false;

        /// <summary>
        ///     Set the number of threads to be used, in case the selected codec implementation supports multi-threading.
        ///     Possible values:
        ///         - 0 - automatically select the number of threads to set
        ///         - integer to max of cpu cores
        ///     Default value is ‘0’.
        /// </summary>
        public int Threads { get; set; } = 0;

        /// <summary>
        ///     Hadware Acceleration
        /// </summary>
        public HWAccel HWAccel { get; set; } = HWAccel.None;

        /// <summary>
        ///     Hardware Acceleration Output Format - Force HWAccel if selected
        /// </summary>
        public bool HWAccelOutputFormatCopy { get; set; } = true;

        /// <summary>
        ///     Audio bit rate
        /// </summary>
        public int? AudioBitRate { get; set; } = null;

        /// <summary>
        ///     Remove Audio
        /// </summary>
        public bool RemoveAudio { get; set; } = false;

        /// <summary>
        ///     Audio sample rate
        /// </summary>
        public AudioSampleRate AudioSampleRate { get; set; } = AudioSampleRate.Default;

        /// <summary>
        ///     The maximum duration
        /// </summary>
        public TimeSpan? MaxVideoDuration { get; set; }

        /// <summary>
        ///     The frame to begin seeking from.
        /// </summary>
        public TimeSpan? Seek { get; set; }

        /// <summary>
        ///     Predefined audio and video options for various file formats,
        ///     <para>Can be used in conjunction with <see cref="TargetStandard" /> option</para>
        /// </summary>
        public Target Target { get; set; } = Target.Default;

        /// <summary>
        ///     Predefined standards to be used with the <see cref="Target" /> option
        /// </summary>
        public TargetStandard TargetStandard { get; set; } = TargetStandard.Default;

        /// <summary>
        ///     Video aspect ratios
        /// </summary>
        public VideoAspectRatio VideoAspectRatio { get; set; } = VideoAspectRatio.Default;

        /// <summary>
        ///     Video bit rate in kbit/s
        /// </summary>
        public int? VideoBitRate { get; set; } = null;
        
         /// <summary>
        ///     Chanel audio
        /// </summary>
        public int? AudioChanel { get; set; } = null;

        /// <summary>
        ///     Video frame rate
        /// </summary>
        public int? VideoFps { get; set; } = null;

        /// <summary>
        ///     Pixel format. Available formats can be gathered via `ffmpeg -pix_fmts`.
        /// </summary>
        public string PixelFormat { get; set; } = null;

        /// <summary>
        ///     Video sizes
        /// </summary>
        public VideoSize VideoSize { get; set; } = VideoSize.Default;

        /// <summary>
        ///     Video sizes
        /// </summary>
        public VideoCodec VideoCodec { get; set; } = VideoCodec.Default;

        /// <summary>
        ///     Codec Preset (Tested for -vcodec libx264)
        /// </summary>
        public VideoCodecPreset VideoCodecPreset { get; set; } = VideoCodecPreset.Default;

        /// <summary>
        ///     Codec Profile (Tested for -vcodec libx264)
        ///     Specifies wheter or not to use a H.264 Profile
        /// </summary>
        public VideoCodecProfile VideoCodecProfile { get; set; } = VideoCodecProfile.Default;

        /// <summary>
        ///     Video sizes
        /// </summary>
        public VideoFormat VideoFormat { get; set; } = VideoFormat.Default;

        /// <summary>
        ///     Video Speed Up / Down using setpts filter
        /// </summary>
        private double? videoTimeScale = null;
        public double? VideoTimeScale { get => videoTimeScale; set => videoTimeScale = (value > 0) ? value : 1; }

        /// <summary>
        ///     Map Metadata from INput to Output
        /// </summary>
        public bool MapMetadata { get; set; } = true;


        /// <summary>
        ///     Custom Width when VideoSize.Custom is set
        /// </summary>
        public int? CustomWidth { get; set; }

        /// <summary>
        ///     Custom Height when VideoSize.Custom is set
        /// </summary>
        public int? CustomHeight { get; set; }

        /// <summary>
        ///     Extra Arguments, such as  -movflags +faststart. Can be used to support missing features temporary
        /// </summary>
        public string ExtraArguments { get; set; }

        /// <summary>
        ///     Specifies an optional rectangle from the source video to crop
        /// </summary>
        public CropRectangle SourceCrop { get; set; }

        /// <summary>
        ///     <para> --- </para>
        ///     <para> Cut audio / video from existing media                </para>
        ///     <para> Example: To cut a 15 minute section                  </para>
        ///     <para> out of a 30 minute video starting                    </para>
        ///     <para> from the 5th minute:                                 </para>
        ///     <para> The start position would be: TimeSpan.FromMinutes(5) </para>
        ///     <para> The length would be:         TimeSpan.FromMinutes(15)</para>
        /// </summary>
        /// <param name="seekToPosition">
        ///     <para>Specify the position to seek to,                  </para>
        ///     <para>if you wish to begin the cut starting             </para>
        ///     <para>from the 5th minute, use: TimeSpan.FromMinutes(5);</para>
        /// </param>
        /// <param name="length">
        ///     <para>Specify the length of the video to cut,           </para>
        ///     <para>to cut out a 15 minute duration                   </para>
        ///     <para>simply use: TimeSpan.FromMinutes(15);             </para>
        /// </param>
        public void CutMedia(TimeSpan seekToPosition, TimeSpan length)
        {
            Seek = seekToPosition;
            MaxVideoDuration = length;
        }
    }
}
