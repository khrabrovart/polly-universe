using PollyUniverse.Shared.Services.Files;
using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Shared.Services;

public interface IMessageHistoryService
{
}

public class MessageHistoryService : IMessageHistoryService
{
    private readonly IMessageHistoryFileService _messageHistoryFileService;

    public MessageHistoryService(IMessageHistoryFileService messageHistoryFileService)
    {
        _messageHistoryFileService = messageHistoryFileService;
    }

    public async Task GetChatHistory(ShortTelegramPeerId peerId)
    {
        var historyFilePath = await _messageHistoryFileService.DownloadMessageHistoryFile(peerId);
    }
}
