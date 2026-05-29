using FFMpegCore;
using FFMpegCore.Exceptions;
using Transcodarr.Node.Common.Models;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class CapabilitiesService(ILogger<CapabilitiesService> logger)
{
    private readonly Dictionary<string, int> _slotsByGroup = new()
    {
        { "software", 1 },
        { "nvenc", 2 },
        { "amf", 1 },
        { "qsv", 1 },
    };

    public async Task<(
        List<EncoderCapability> Encoders,
        Dictionary<string, int> GroupCapacities
    )> GetEncodersAsync(CancellationToken stoppingToken)
    {
        var encoders = new List<EncoderCapability>();
        foreach (var kvp in TranscodersMapping.EncodersByCodec)
        {
            foreach (var possibleEncoder in kvp.Value)
            {
                if (!await IsEncoderAvailableAsync(possibleEncoder))
                    continue;

                stoppingToken.ThrowIfCancellationRequested();

                encoders.Add(
                    new EncoderCapability
                    {
                        EncoderName = possibleEncoder,
                        CodecType = kvp.Key,
                        SlotGroup = TranscodersMapping.GetSlotGroup(possibleEncoder),
                    }
                );
            }
        }

        var groupCapacities = encoders
            .Select(e => e.SlotGroup)
            .Distinct()
            .ToDictionary(g => g, g => _slotsByGroup.GetValueOrDefault(g, 1));

        return (encoders, groupCapacities);
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
