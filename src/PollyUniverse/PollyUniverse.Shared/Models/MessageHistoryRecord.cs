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

    public TelegramShortPeerId PeerId { get; init; }

    public IReadOnlyCollection<MessageHistoryRecord> Messages => _messages;

    public void AddMessage(MessageHistoryRecord message)
    {
        _messages.Add(message);
    }
}

public record MessageHistoryRecord
{
    public DateTime Date { get; init; }

    public long SenderId { get; init; }

    public string SenderFirstName { get; init; }

    public string SenderLastName { get; init; }

    public string SenderUsername { get; init; }

    public string Text { get; init; }
}
