namespace Transcodarr.Shared;

public class NodeInfo
{
    public required string Name { get; init; }
    public IReadOnlyCollection<EncoderCapability> EncoderCapabilities { get; init; } = [];

    public IReadOnlyDictionary<EncoderGroup, int> SlotGroupCapacities { get; init; } =
        new Dictionary<EncoderGroup, int>();
}
