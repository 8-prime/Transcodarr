namespace Transcodearr.Shared;

public static class Encoders
{
    public const string CpuEncode = "libx265";
    public const string NvidiaEncode = "hevc_nvenc";
    public const string IntelEncode = "hevc_qsv";
}