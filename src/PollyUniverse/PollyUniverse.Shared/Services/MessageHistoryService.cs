using PollyUniverse.Shared.Services.Files;
using PollyUniverse.Shared.Telegram.Models;
using PollyUniverse.Shared.Models;
using CsvHelper;
using System.Globalization;

namespace PollyUniverse.Shared.Services;

public interface IMessageHistoryService
{
    Task<MessageHistory> GetHistory(TelegramShortPeerId peerId);

    Task SaveHistory(MessageHistory history);
}

public class MessageHistoryService : IMessageHistoryService
{
    private readonly IMessageHistoryFileService _messageHistoryFileService;

    public MessageHistoryService(IMessageHistoryFileService messageHistoryFileService)
    {
        _messageHistoryFileService = messageHistoryFileService;
    }

    public async Task<MessageHistory> GetHistory(TelegramShortPeerId peerId)
    {
        var historyFilePath = await _messageHistoryFileService.DownloadMessageHistoryFile(peerId);

        if (string.IsNullOrEmpty(historyFilePath))
        {
            return new MessageHistory(peerId, []);
        }

        using var reader = new StreamReader(historyFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<MessageHistoryRecord>();
        return new MessageHistory(peerId, records);
    }

    public async Task SaveHistory(MessageHistory history)
    {
        var (_, localFilePath) = _messageHistoryFileService.GetFilePaths(history.PeerId);

        var directory = Path.GetDirectoryName(localFilePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var writer = new StreamWriter(localFilePath, append: false);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(history.Messages);
        await writer.FlushAsync();

        var uploaded = await _messageHistoryFileService.UploadMessageHistoryFile(history.PeerId);

        if (!uploaded)
        {
            throw new Exception("Failed to upload message history file");
        }
    }
}
