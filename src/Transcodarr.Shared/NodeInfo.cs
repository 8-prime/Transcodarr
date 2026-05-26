namespace Transcodarr.Shared;

public class NodeInfo
{
    public required string Name { get; init; }
    public IReadOnlyCollection<EncoderCapability> EncoderCapabilities { get; init; } = [];
    public int Slots { get; set; }
}
