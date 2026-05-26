using FFMpegCore;
using Transcodarr.Node.Common.Mapping;
using Transcodarr.Node.Services.Connection;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class TranscodeService
{
    private readonly MessagesQueue _messagesQueue;

    public TranscodeService(MessagesQueue messagesQueue)
    {
        _messagesQueue = messagesQueue;
    }

    public async Task<TranscodeResponse> RunTranscodeAsync(
        Guid jobLeaseId,
        string filePath,
        string outputPath,
        TimeSpan duration,
        TranscodeQualitySettings transcodeQualitySettings,
        string encoderName,
        CancellationToken stoppingToken
    )
    {
        var success = await FFMpegArguments
            .FromFileInput(filePath)
            .OutputToFile(
                outputPath,
                false,
                options =>
                    options
                        .WithVideoCodec(transcodeQualitySettings.DesiredVideoCodec.Map())
                        .WithConstantRateFactor(transcodeQualitySettings.ConstantRateFactor)
                        .WithAudioCodec(transcodeQualitySettings.DesiredAudioCodec.Map())
                        .WithFastStart()
            )
            .NotifyOnProgress(
                p =>
                    _messagesQueue.Enqueue(
                        new TranscodeProgress(p, jobLeaseId) { CorrelationId = Guid.NewGuid() }
                    ),
                duration
            )
            .ProcessAsynchronously();

        var fi = new FileInfo(outputPath);

        return new TranscodeResponse(
            jobLeaseId,
            success,
            new TranscoderSnapshot(
                encoderName,
                transcodeQualitySettings.DesiredAudioCodec,
                transcodeQualitySettings.DesiredVideoCodec,
                transcodeQualitySettings.DesiredEncoderPreset,
                transcodeQualitySettings.ConstantRateFactor
            ),
            fi.Length,
            0
        )
        {
            CorrelationId = Guid.NewGuid(),
        };
    }
}
