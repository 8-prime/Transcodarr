using FFMpegCore;
using FFMpegCore.Exceptions;
using Transcodarr.Node.Common.Models;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class CapabilitiesService(ILogger<CapabilitiesService> logger)
{
    private readonly Dictionary<string, int> _defaultEncoderSlots = new()
    {
        { "libx265", 1 },
        { "hevc_nvenc", 2 },
        { "hevc_amf", 1 },
        { "hevc_qsv", 1 },
    };

    public async Task<List<EncoderCapability>> GetEncodersAsync(CancellationToken stoppingToken)
    {
        var capabilities = new List<EncoderCapability>();
        foreach (var kvp in TranscodersMapping.EncodersByCodec)
        {
            foreach (var possibleEncoder in kvp.Value)
            {
                if (!await IsEncoderAvailableAsync(possibleEncoder))
                {
                    continue;
                }

                stoppingToken.ThrowIfCancellationRequested();

                capabilities.Add(
                    new EncoderCapability
                    {
                        EncoderName = possibleEncoder,
                        CodecType = kvp.Key,
                        Slots = _defaultEncoderSlots.GetValueOrDefault(possibleEncoder, 1),
                    }
                );
            }
        }

        return capabilities;
    }

    private async Task<bool> IsEncoderAvailableAsync(string encoderName)
    {
        var nullSink = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
        try
        {
            return await FFMpegArguments
                .FromFileInput(
                    "color=c=black:s=320x240:r=30:d=1",
                    false,
                    o => o.ForceFormat("lavfi")
                )
                .OutputToFile(
                    nullSink,
                    true,
                    o => o.WithVideoCodec(encoderName).ForceFormat("null")
                )
                .ProcessAsynchronously();
        }
        catch (FFMpegException ex)
        {
            logger.LogError(
                ex,
                "An error occured during FFMpeg processing of encoder check for {EncoderName}",
                encoderName
            );
            return false;
        }
    }
}
