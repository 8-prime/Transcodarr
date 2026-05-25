using FFMpegCore;
using FFMpegCore.Exceptions;
using Transcodearr.Shared;

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
        var capabilities = new List<EncoderCapability>(_defaultEncoderSlots.Count);
        foreach (var possibleEncoder in _defaultEncoderSlots.Keys)
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
                    Slots = _defaultEncoderSlots.GetValueOrDefault(possibleEncoder, 1),
                }
            );
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
                    "color=c=black:s=1920x1080:r=1:d=1",
                    false,
                    o => o.ForceFormat("lavfi")
                )
                .OutputToFile(
                    nullSink,
                    true,
                    o =>
                        o.WithVideoCodec(encoderName)
                            .WithCustomArgument("-vframes 1")
                            .ForceFormat("null")
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
