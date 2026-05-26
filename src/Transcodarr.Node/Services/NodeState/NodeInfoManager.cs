using System.Collections.ObjectModel;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class NodeInfoManager
{
    private readonly List<EncoderCapability> _capabilities = [];

    public void Initialize(List<EncoderCapability> capabilities)
    {
        _capabilities.Clear();
        _capabilities.AddRange(capabilities);
    }

    public ReadOnlyCollection<EncoderCapability> Capabilities => _capabilities.AsReadOnly();
}
