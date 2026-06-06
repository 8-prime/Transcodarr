using FFMpegCore;
using FFMpegCore.Enums;
using Transcodarr.Node.Common.Mapping;
using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services.Connection;
using Transcodarr.Shared;
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

        var encoderGroup = TranscodersMapping.GetSlotGroup(encoderName);

        var args = CreateTranscodeArgs(
            filePath,
            outputPath,
            transcodeQualitySettings,
            encoderName,
            encoderGroup
        );
        SetupNotifications(jobLeaseId, duration, args).CancellableThrough(stoppingToken);

        _logger.LogDebug("Running FFMpeg with args {Args}", args.Arguments);
        var success = await args.ProcessAsynchronously(throwOnError: false);
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

    private FFMpegArgumentProcessor SetupNotifications(
        Guid jobLeaseId,
        TimeSpan duration,
        FFMpegArgumentProcessor args
    )
    {
        return args.NotifyOnProgress(
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
            );
    }

    private FFMpegArgumentProcessor CreateTranscodeArgs(
        string filePath,
        string outputPath,
        TranscodeQualitySettings transcodeQualitySettings,
        string encoderName,
        EncoderGroup encoderGroup
    )
    {
        return FFMpegArguments
            .FromFileInput(
                filePath,
                true,
                inputOptions =>
                {
                    switch (encoderGroup)
                    {
                        case EncoderGroup.Nvenc:
                            inputOptions
                                .WithHardwareAcceleration(HardwareAccelerationDevice.CUVID)
                                .WithCustomArgument("-hwaccel_output_format cuda");
                            break;
                        case EncoderGroup.Qsv:
                            inputOptions
                                .WithHardwareAcceleration(HardwareAccelerationDevice.QSV)
                                .WithCustomArgument("-hwaccel_output_format qsv");
                            break;
                        case EncoderGroup.Amf:
                            inputOptions.WithHardwareAcceleration();
                            break;
                        case EncoderGroup.Software:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            )
            .OutputToFile(
                outputPath,
                false,
                options =>
                {
                    var preset = transcodeQualitySettings.DesiredEncoderPreset;
                    var crf = transcodeQualitySettings.ConstantRateFactor;

                    options
                        .WithVideoCodec(encoderName)
                        .WithAudioCodec(transcodeQualitySettings.DesiredAudioCodec.Map())
                        .OverwriteExisting();

                    switch (encoderGroup)
                    {
                        case EncoderGroup.Nvenc:
                            options
                                .WithCustomArgument($"-preset {preset.MapToNvenc()}")
                                .WithCustomArgument($"-cq {crf}");
                            break;
                        case EncoderGroup.Qsv:
                            options
                                .WithCustomArgument($"-preset {preset.MapToQsv()}")
                                .WithCustomArgument($"-global_quality {crf}")
                                .WithCustomArgument("-look_ahead 1");
                            break;
                        case EncoderGroup.Software:
                        case EncoderGroup.Amf:
                        default:
                            options.WithSpeedPreset(preset.Map()).WithConstantRateFactor(crf);
                            break;
                    }
                }
            );
    }
}
