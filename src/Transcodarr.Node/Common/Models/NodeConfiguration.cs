namespace Transcodarr.Node.Common.Models;

public class NodeConfiguration
{
    public required string CoreUrl { get; set; }
    public required string NodeId { get; set; }
    public Dictionary<string, int> EncoderTypeCapacities { get; set; } = [];
}
