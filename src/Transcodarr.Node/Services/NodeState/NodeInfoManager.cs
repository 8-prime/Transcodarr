using System.Collections.ObjectModel;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class NodeInfoManager
{
    private readonly List<EncoderCapability> _capabilities = [];
    private Dictionary<EncoderGroup, int> _groupCapacities = new();

    public void Initialize(
        List<EncoderCapability> capabilities,
        Dictionary<EncoderGroup, int> groupCapacities
    )
    {
        _capabilities.Clear();
        _capabilities.AddRange(capabilities);
        _groupCapacities = groupCapacities;
    }

    public ReadOnlyCollection<EncoderCapability> Capabilities => _capabilities.AsReadOnly();
    public IReadOnlyDictionary<EncoderGroup, int> GroupCapacities => _groupCapacities;
}
