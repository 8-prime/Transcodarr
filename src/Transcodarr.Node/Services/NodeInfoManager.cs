using System.Collections.ObjectModel;
using Transcodearr.Shared;

namespace Transcodarr.Node.Services;

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