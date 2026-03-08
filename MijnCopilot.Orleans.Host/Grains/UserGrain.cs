using MijnCopilot.Contracts.Grains;
using MijnCopilot.Contracts.Model;

namespace MijnCopilot.Orleans.Host.Grains;

public class UserGrain : Grain, IUserGrain
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private DateTime _lastSeenOn = DateTime.UtcNow;
    private readonly List<Guid> _chatIds = [];

    public Task<UserInfo> GetInfoAsync()
    {
        _lastSeenOn = DateTime.UtcNow;
        return Task.FromResult(new UserInfo
        {
            UserId = this.GetPrimaryKeyString(),
            Name = _name,
            Email = _email,
            LastSeenOn = _lastSeenOn
        });
    }

    public Task SetInfoAsync(string name, string email)
    {
        _name = name;
        _email = email;
        _lastSeenOn = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Guid>> GetChatIdsAsync()
        => Task.FromResult<IReadOnlyList<Guid>>(_chatIds.AsReadOnly());

    public Task AddChatAsync(Guid chatId)
    {
        if (!_chatIds.Contains(chatId))
            _chatIds.Add(chatId);
        return Task.CompletedTask;
    }

    public Task RemoveChatAsync(Guid chatId)
    {
        _chatIds.Remove(chatId);
        return Task.CompletedTask;
    }
}
