using MijnCopilot.Contracts.Grains;
using MijnCopilot.Contracts.Model;
using Orleans.Placement;

namespace MijnCopilot.Orleans.Host.Grains;

[GenerateSerializer]
public class UserGrainState
{
    [Id(0)] public string Name { get; set; } = string.Empty;
    [Id(1)] public string Email { get; set; } = string.Empty;
    [Id(2)] public DateTime LastSeenOn { get; set; }
    [Id(3)] public List<Guid> ChatIds { get; set; } = [];
}

[PreferLocalPlacement]
public class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserGrainState> _state;

    public UserGrain([PersistentState("state", "blob-store")] IPersistentState<UserGrainState> state)
    {
        _state = state;
    }

    private UserGrainState State => _state.State;

    public Task<UserInfo> GetInfoAsync()
        => Task.FromResult(new UserInfo
        {
            UserId = this.GetPrimaryKeyString(),
            Name = State.Name,
            Email = State.Email,
            LastSeenOn = State.LastSeenOn
        });

    public async Task SetInfoAsync(string name, string email)
    {
        State.Name = name;
        State.Email = email;
        State.LastSeenOn = DateTime.UtcNow;
        await _state.WriteStateAsync();
    }

    public Task<IReadOnlyList<Guid>> GetChatIdsAsync()
        => Task.FromResult<IReadOnlyList<Guid>>(State.ChatIds.AsReadOnly());

    public async Task AddChatAsync(Guid chatId)
    {
        if (!State.ChatIds.Contains(chatId))
        {
            State.ChatIds.Add(chatId);
            await _state.WriteStateAsync();
        }
    }

    public async Task RemoveChatAsync(Guid chatId)
    {
        if (State.ChatIds.Remove(chatId))
            await _state.WriteStateAsync();
    }

    public Task DeactivateAsync()
    {
        DeactivateOnIdle();
        return Task.CompletedTask;
    }
}
