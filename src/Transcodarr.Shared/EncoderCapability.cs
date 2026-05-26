namespace Transcodarr.Shared;

public class EncoderCapability
{
    public required string EncoderName { get; init; }
    public int Slots { get; init; }
}
