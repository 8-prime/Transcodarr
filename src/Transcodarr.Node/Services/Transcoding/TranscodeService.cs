using FFMpegCore;
using Transcodarr.Node.Common.Mapping;
using Transcodarr.Shared.DTOs;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class TranscodeService
{
    public async Task<TranscodeResponse> RunTranscodeAsync(Guid jobLeaseId, string filePath, string outputPath,
        TranscodeQualitySettings transcodeQualitySettings, string encoderName, CancellationToken stoppingToken)
    {
        var success = await FFMpegArguments
            .FromFileInput(filePath)
            .OutputToFile(outputPath, false, options => options
                .WithVideoCodec(transcodeQualitySettings.DesiredVideoCodec.Map())
                .WithConstantRateFactor(transcodeQualitySettings.ConstantRateFactor)
                .WithAudioCodec(transcodeQualitySettings.DesiredAudioCodec.Map())
                .WithFastStart())
            .NotifyOnProgress((p) => { })
            //TODO: use notify progress and send update to core.NotifyOnProgress()
            .ProcessAsynchronously();

        var fi = new FileInfo(outputPath);

        return new TranscodeResponse(jobLeaseId, success,
            new TranscoderSnapshot(encoderName, transcodeQualitySettings.DesiredAudioCodec,
                transcodeQualitySettings.DesiredVideoCodec, transcodeQualitySettings.DesiredEncoderPreset,
                transcodeQualitySettings.ConstantRateFactor), fi.Length, 0)
        {
            CorrelationId = Guid.NewGuid(),
        };
    }
}
