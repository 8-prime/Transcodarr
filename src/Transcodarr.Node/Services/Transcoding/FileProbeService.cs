using FFMpegCore;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class FileProbeService
{
    public async Task<FileProbeResult?> ProbeFileAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var mediaInfo = await FFProbe.AnalyseAsync(path, cancellationToken: cancellationToken);
        var mainVideoStream = mediaInfo.PrimaryVideoStream;
        if (mainVideoStream == null)
        {
            //TODO: what now?
            return null;
        }

        return new FileProbeResult
        {
            AudioStreams = string.Join(
                ',',
                mediaInfo.AudioStreams.Select(a => a.Language).OfType<string>()
            ),
            VideoCodec = mainVideoStream.CodecName,
            Bitrate = mainVideoStream.BitRate,
            Duration = mainVideoStream.Duration,
            Height = mainVideoStream.Height,
            Width = mainVideoStream.Width,
            IsHdr =
                mainVideoStream.PixelFormat.Contains("10le")
                || mainVideoStream.PixelFormat.Contains("10be"),
        };
    }
}
