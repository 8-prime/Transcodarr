using FFMpegCore;
using Transcodarr.Node.Common.Mapping;
using Transcodarr.Node.Services.Connection;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class TranscodeService
{
    private readonly MessagesQueue _messagesQueue;
    private readonly ILogger<TranscodeService> _logger;

    public TranscodeService(MessagesQueue messagesQueue, ILogger<TranscodeService> logger)
    {
        _messagesQueue = messagesQueue;
        _logger = logger;
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
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        _logger.LogInformation(
            "Job {JobId}: starting ffmpeg — input={Input} output={Output} encoder={Encoder} crf={Crf} duration={Duration}",
            jobLeaseId,
            filePath,
            outputPath,
            encoderName,
            transcodeQualitySettings.ConstantRateFactor,
            duration
        );

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
                {
                    _logger.LogDebug("Job {JobId}: progress {Percent:F1}%", jobLeaseId, p);
                    _messagesQueue.Enqueue(
                        new TranscodeProgress(p, jobLeaseId) { CorrelationId = Guid.NewGuid() }
                    );
                },
                duration
            )
            .NotifyOnError(e =>
                _logger.LogWarning("Job {JobId} ffmpeg stderr: {Line}", jobLeaseId, e)
            )
            .ProcessAsynchronously(throwOnError: false);

        _logger.LogInformation("Job {JobId}: ffmpeg exited success={Success}", jobLeaseId, success);

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
