using FFMpegCore;
using Transcodarr.Node.Common.Mapping;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Node.Services;

public class TranscodeService
{
    public async Task RunTranscodeAsync(string filePath, string outputPath,
        TranscodeQualitySettings transcodeQualitySettings, CancellationToken stoppingToken)
    {
        var success = await FFMpegArguments
            .FromFileInput(filePath)
            .OutputToFile(outputPath, false, options => options
                .WithVideoCodec(transcodeQualitySettings.DesiredVideoCodec.Map())
                .WithConstantRateFactor(transcodeQualitySettings.ConstantRateFactor)
                .WithAudioCodec(transcodeQualitySettings.DesiredAudioCodec.Map())
                .WithFastStart())
            //TODO: use notify progress and send update to core.NotifyOnProgress()
            .ProcessAsynchronously();
    }
}