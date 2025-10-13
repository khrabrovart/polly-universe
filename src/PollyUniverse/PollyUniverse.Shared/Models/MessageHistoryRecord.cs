using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Shared.Models;

public record MessageHistory
{
    private readonly List<MessageHistoryRecord> _messages;

    public MessageHistory(TelegramShortPeerId peerId, IEnumerable<MessageHistoryRecord> messages)
    {
        _messages = messages.ToList();
        PeerId = peerId;
    }

    public TelegramShortPeerId PeerId { get; }

    public IReadOnlyCollection<MessageHistoryRecord> Messages => _messages;

    public void AddMessage(MessageHistoryRecord message)
    {
        _messages.Add(message);
    }
}

public enum MessageHistoryRole
{
    User,
    Assistant
}

public record MessageHistoryRecord
{
    public required DateTime Date { get; init; }

    public required MessageHistoryRole Role { get; init; }

    public required long SenderId { get; init; }

    public required string SenderName { get; init; }

    public required string SenderUsername { get; init; }

    public required string Text { get; init; }
}
