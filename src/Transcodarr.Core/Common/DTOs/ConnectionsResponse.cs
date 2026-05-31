using Transcodarr.Shared;

namespace Transcodarr.Core.Common.DTOs;

public class ConnectionsResponse
{
    public required string ConnectionId { get; set; }
    public NodeInfo? NodeInfo { get; set; }
    public int FreeSlots { get; set; }
    public bool ConnectionIsReady { get; set; }
}
