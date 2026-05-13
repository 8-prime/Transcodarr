using Transcodarr.Core.Common.Models;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class MessageHandler
{
    public async Task ProcessMessageAsync(SocketMessage message, NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken)
    {
        switch (message)
        {
            case NodeInfoMessage nodeInfoMessage:
                nodeConnectionInfo.NodeInfo = nodeInfoMessage.Info;
                break;
            default:
                break;
        }
    }
}