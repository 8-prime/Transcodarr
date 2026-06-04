using FFMpegCore;
using FFMpegCore.Exceptions;
using Microsoft.Extensions.Options;
using Transcodarr.Node.Common.Models;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class CapabilitiesService(
    ILogger<CapabilitiesService> logger,
    IOptions<NodeConfiguration> configuration
)
{
    private readonly Dictionary<EncoderGroup, int> _slotsByGroup = new()
    {
        {
            EncoderGroup.Software,
            configuration.Value.EncoderTypeCapacities.TryGetValue(
                EncoderGroup.Software,
                out var softwareSlots
            )
                ? softwareSlots
                : 1
        },
        {
            EncoderGroup.Nvenc,
            configuration.Value.EncoderTypeCapacities.TryGetValue(
                EncoderGroup.Nvenc,
                out var nvencSlots
            )
                ? nvencSlots
                : 2
        },
        {
            EncoderGroup.Amf,
            configuration.Value.EncoderTypeCapacities.TryGetValue(
                EncoderGroup.Amf,
                out var amfSlots
            )
                ? amfSlots
                : 1
        },
        {
            EncoderGroup.Qsv,
            configuration.Value.EncoderTypeCapacities.TryGetValue(
                EncoderGroup.Qsv,
                out var qsvSlots
            )
                ? qsvSlots
                : 1
        },
    };

    public async Task<(
        List<EncoderCapability> Encoders,
        Dictionary<EncoderGroup, int> GroupCapacities
        )> GetEncodersAsync(CancellationToken stoppingToken)
    {
        var encoders = new List<EncoderCapability>();
        foreach (var kvp in TranscodersMapping.EncodersByCodec)
        {
            foreach (var possibleEncoder in kvp.Value)
            {
                if (!await IsEncoderAvailableAsync(possibleEncoder, stoppingToken))
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

    private async Task<bool> IsEncoderAvailableAsync(
        string encoderName,
        CancellationToken stoppingToken
    )
    {
        var nullSink = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
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
                .CancellableThrough(cts.Token)
                .ProcessAsynchronously();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Encoder {EncoderName} did not finish the test encode in time",
                encoderName
            );
            return false;
        }
        catch (Exception ex)
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