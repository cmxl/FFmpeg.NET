namespace FFmpeg.NET.Enums
{
    public enum HWAccel
    {
        None,
        cuda,
        cuvid,
        dxva2,               //Direct-X Video Acceleration API, developed by Microsoft (supports Windows and XBox360).
        qsv,
        d3d11va}
}