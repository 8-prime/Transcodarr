using System.Text.RegularExpressions;

namespace Transcodarr.Core.Common.Constants;

public static partial class FileTypeConstants
{
    [GeneratedRegex("mp4|mpeg4|mkv", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex IsVideoFileRegex();

    public const string TempFileSuffix = "transcodarr.";
}
