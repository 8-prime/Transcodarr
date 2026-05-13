namespace Transcodearr.Shared;

public class NodeInfo
{
    public required string Name { get; init; }
    public List<EncoderCapability> EncoderCapabilities { get; init; } = [];
}