using Transcodarr.Shared.DTOs;

namespace Transcodarr.Shared;

public class EncoderCapability
{
    public required string EncoderName { get; init; }
    public VideoCodec CodecType { get; init; }
    public required EncoderGroup SlotGroup { get; init; }
}
