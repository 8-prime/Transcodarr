using System.Threading.Channels;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services;

public class TranscodesQueue
{
    public Channel<TranscodeRequest> TranscodeRequests { get; init; } = Channel.CreateUnbounded<TranscodeRequest>();
}