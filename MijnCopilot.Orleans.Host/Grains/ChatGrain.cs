using MijnCopilot.Contracts.Grains;
using MijnCopilot.Contracts.Model;
using MijnCopilot.Model;

namespace MijnCopilot.Orleans.Host.Grains;

public class ChatGrain : Grain, IChatGrain
{
    private string _userId = string.Empty;
    private string _title = string.Empty;
    private bool _isArchived;
    private DateTime _startedOn;
    private DateTime _lastActivityOn;
    private readonly List<ChatMessageInfo> _history = [];

    public Task<ChatInfo> GetInfoAsync()
        => Task.FromResult(new ChatInfo
        {
            Id = this.GetPrimaryKey(),
            UserId = _userId,
            Title = _title,
            IsArchived = _isArchived,
            StartedOn = _startedOn,
            LastActivityOn = _lastActivityOn
        });

    public Task InitializeAsync(string userId, string title)
    {
        if (_startedOn != default)
            return Task.CompletedTask;

        _userId = userId;
        _title = title;
        _startedOn = DateTime.UtcNow;
        _lastActivityOn = _startedOn;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatMessageInfo>> GetHistoryAsync()
        => Task.FromResult<IReadOnlyList<ChatMessageInfo>>(_history.AsReadOnly());

    public Task AddMessageAsync(MessageType type, string content, string? agentName = null, int tokensUsed = 0)
    {
        _history.Add(new ChatMessageInfo
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            AgentName = agentName,
            TokensUsed = tokensUsed,
            PostedOn = DateTime.UtcNow
        });
        _lastActivityOn = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task ArchiveAsync()
    {
        _isArchived = true;
        return Task.CompletedTask;
    }
}
