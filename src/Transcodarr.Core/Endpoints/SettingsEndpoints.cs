using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Transcodarr.Core.Common.DTOs;
using Transcodarr.Core.Services.Configuration;

namespace Transcodarr.Core.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("settings");
        group.MapGet("", GetSettings);
        group.MapPut("", PutSettings);

        return endpoints;
    }

    private static Results<Ok<AppSettingsResponse>, BadRequest<string>> GetSettings(
        [FromServices] ConfigurationService configurationService
    )
    {
        if (!configurationService.Initialized)
        {
            return TypedResults.BadRequest("Initial configuration must be set");
        }

        var internalConfig = configurationService.Current;
        return TypedResults.Ok(
            new AppSettingsResponse
            {
                AudioCodec = internalConfig.TranscodeAudioCodec,
                VideoCodec = internalConfig.TranscodeVideoCodec,
                AutoApplyTranscode = internalConfig.AutoApplyTranscode,
                Crf = internalConfig.ConstantRateFactor,
                Preset = internalConfig.TranscodeEncoderPreset,
                JobExpirationInMinutes = internalConfig.JobExpirationInMinutes,
                TranscodeTempDirectory = internalConfig.TranscodeTempDirectory,
            }
        );
    }

    private static async Task<
        Results<Ok<AppSettingsResponse>, InternalServerError<string>>
    > PutSettings(
        [FromBody] UpdateAppSettingsRequest updateAppSettingsRequest,
        [FromServices] ConfigurationService configurationService,
        CancellationToken stoppingToken
    )
    {
        await configurationService.UpdateAsync(
            c =>
            {
                c.TranscodeAudioCodec = updateAppSettingsRequest.AudioCodec;
                c.TranscodeVideoCodec = updateAppSettingsRequest.VideoCodec;
                c.AutoApplyTranscode = updateAppSettingsRequest.AutoApplyTranscode;
                c.ConstantRateFactor = updateAppSettingsRequest.Crf;
                c.TranscodeEncoderPreset = updateAppSettingsRequest.Preset;
                c.JobExpirationInMinutes = updateAppSettingsRequest.JobExpirationInMinutes;
                c.TranscodeTempDirectory = updateAppSettingsRequest.TranscodeTempDirectory;
            },
            stoppingToken
        );

        var internalConfig = configurationService.Current;
        return TypedResults.Ok(
            new AppSettingsResponse
            {
                AudioCodec = internalConfig.TranscodeAudioCodec,
                VideoCodec = internalConfig.TranscodeVideoCodec,
                AutoApplyTranscode = internalConfig.AutoApplyTranscode,
                Crf = internalConfig.ConstantRateFactor,
                Preset = internalConfig.TranscodeEncoderPreset,
                JobExpirationInMinutes = internalConfig.JobExpirationInMinutes,
                TranscodeTempDirectory = internalConfig.TranscodeTempDirectory,
            }
        );
    }
}
