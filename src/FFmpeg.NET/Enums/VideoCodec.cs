namespace FFmpeg.NET.Enums
{
    //Enums compatibility considerations:
    //Beggining with '_' must be removed
    //Doble underscore '__' must be replaced by '-'
    public enum VideoCodec
    {
        Default,
        copy,
        a64multi,                                //Multicolor charset for Commodore 64 (codec a64_multi)
        a64multi5,                                //Multicolor charset for Commodore 64, extended with 5th color (colram) (codec a64_multi5)
        alias_pix,                                //Alias/Wavefront PIX image
        amv,                                //AMV Video
        apng,                                //APNG (Animated Portable Network Graphics) image
        asv1,                                //ASUS V1
        asv2,                                //ASUS V2
        libaom__av1,                                //libaom AV1 (codec av1)
        avrp,                                //Avid 1:1 10-bit RGB Packer
        avui,                                //Avid Meridien Uncompressed
        ayuv,                                //Uncompressed packed MS 4:4:4:4
        bmp,                                //BMP (Windows and OS/2 bitmap)
        cinepak,                                //Cinepak
        cljr,                                //Cirrus Logic AccuPak
        vc2,                                //SMPTE VC-2 (codec dirac)
        dnxhd,                                //VC3/DNxHD
        dpx,                                //DPX (Digital Picture Exchange) image
        dvvideo,                                //DV (Digital Video)
        ffv1,                                //FFmpeg video codec #1
        ffvhuff,                                //Huffyuv FFmpeg variant
        fits,                                //Flexible Image Transport System
        flashsv,                                //Flash Screen Video
        flashsv2,                                //Flash Screen Video Version 2
        flv,                                //FLV / Sorenson Spark / Sorenson H.263 (Flash Video) (codec flv1)
        gif,                                //GIF (Graphics Interchange Format)
        h261,                                //H.261
        h263,                                //H.263 / H.263-1996
        h263p,                                //H.263+ / H.263-1998 / H.263 version 2
        libx264,                                //libx264 H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (codec h264)
        libx264rgb,                                //libx264 H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 RGB (codec h264)
        h264_amf,                                //AMD AMF H.264 Encoder (codec h264)
        h264_nvenc,                                //NVIDIA NVENC H.264 encoder (codec h264)
        h264_qsv,                                //H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (Intel Quick Sync Video acceleration) (codec h264)
        nvenc,                                //NVIDIA NVENC H.264 encoder (codec h264)
        nvenc_h264,                                //NVIDIA NVENC H.264 encoder (codec h264)
        hap,                                //Vidvox Hap
        libx265,                                //libx265 H.265 / HEVC (codec hevc)
        nvenc_hevc,                                //NVIDIA NVENC hevc encoder (codec hevc)
        hevc_amf,                                //AMD AMF HEVC encoder (codec hevc)
        hevc_nvenc,                                //NVIDIA NVENC hevc encoder (codec hevc)
        hevc_qsv,                                //HEVC (Intel Quick Sync Video acceleration) (codec hevc)
        huffyuv,                                //Huffyuv / HuffYUV
        jpeg2000,                                //JPEG 2000
        libopenjpeg,                                //OpenJPEG JPEG 2000 (codec jpeg2000)
        jpegls,                                //JPEG-LS
        ljpeg,                                //Lossless JPEG
        magicyuv,                                //MagicYUV video
        mjpeg,                                //MJPEG (Motion JPEG)
        mjpeg_qsv,                                //MJPEG (Intel Quick Sync Video acceleration) (codec mjpeg)
        mpeg1video,                                //MPEG-1 video
        mpeg2video,                                //MPEG-2 video
        mpeg2_qsv,                                //MPEG-2 video (Intel Quick Sync Video acceleration) (codec mpeg2video)
        mpeg4,                                //MPEG-4 part 2
        libxvid,                                //libxvidcore MPEG-4 part 2 (codec mpeg4)
        msmpeg4v2,                                //MPEG-4 part 2 Microsoft variant version 2
        msmpeg4,                                //MPEG-4 part 2 Microsoft variant version 3 (codec msmpeg4v3)
        msvideo1,                                //Microsoft Video-1
        pam,                                //PAM (Portable AnyMap) image
        pbm,                                //PBM (Portable BitMap) image
        pcx,                                //PC Paintbrush PCX image
        pgm,                                //PGM (Portable GrayMap) image
        pgmyuv,                                //PGMYUV (Portable GrayMap YUV) image
        png,                                //PNG (Portable Network Graphics) image
        ppm,                                //PPM (Portable PixelMap) image
        prores,                                //Apple ProRes
        prores_aw,                                //Apple ProRes (codec prores)
        prores_ks,                                //Apple ProRes (iCodec Pro) (codec prores)
        qtrle,                                //QuickTime Animation (RLE) video
        r10k,                                //AJA Kona 10-bit RGB Codec
        r210,                                //Uncompressed RGB 10-bit
        rawvideo,                                //raw video
        roqvideo,                                //id RoQ video (codec roq)
        rv10,                                //RealVideo 1.0
        rv20,                                //RealVideo 2.0
        sgi,                                //SGI image
        snow,                                //Snow
        sunrast,                                //Sun Rasterfile image
        svq1,                                //Sorenson Vector Quantizer 1 / Sorenson Video 1 / SVQ1
        targa,                                //Truevision Targa image
        libtheora,                                //libtheora Theora (codec theora)
        tiff,                                //TIFF image
        utvideo,                                //Ut Video
        v210,                                //Uncompressed 4:2:2 10-bit
        v308,                                //Uncompressed packed 4:4:4
        v408,                                //Uncompressed packed QT 4:4:4:4
        v410,                                //Uncompressed 4:4:4 10-bit
        libvpx,                                //libvpx VP8 (codec vp8)
        libvpx__vp9,                                //libvpx VP9 (codec vp9)
        vp9_qsv,                                //VP9 video (Intel Quick Sync Video acceleration) (codec vp9)
        libwebp_anim,                                //libwebp WebP image (codec webp)
        libwebp,                                //libwebp WebP image (codec webp)
        wmv1,                                //Windows Media Video 7
        wmv2,                                //Windows Media Video 8
        wrapped_avframe,                                //AVFrame to AVPacket passthrough
        xbm,                                //XBM (X BitMap) image
        xface,                                //X-face image
        xwd,                                //XWD (X Window Dump) image
        y41p,                                //Uncompressed YUV 4:1:1 12-bit
        yuv4,                                //Uncompressed packed 4:2:0
        zlib,                                //LCL (LossLess Codec Library) ZLIB
        zmbv,                                //Zip Motion Blocks Video
    }
}