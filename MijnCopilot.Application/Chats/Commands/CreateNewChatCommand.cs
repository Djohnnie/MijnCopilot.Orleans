using MediatR;
using MijnCopilot.Contracts.Grains;
using MijnCopilot.Model;

namespace MijnCopilot.Application.Chats.Commands;

public class CreateNewChatCommand : IRequest<CreateNewChatResponse>
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Request { get; set; }
    public int TokensUsed { get; set; }
}

public class CreateNewChatResponse
{
    public Guid ChatId { get; set; }
}

public class CreateNewChatCommandHandler : IRequestHandler<CreateNewChatCommand, CreateNewChatResponse>
{
    private readonly IGrainFactory _grainFactory;

    public CreateNewChatCommandHandler(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<CreateNewChatResponse> Handle(CreateNewChatCommand request, CancellationToken cancellationToken)
    {
        var chatId = Guid.NewGuid();

        var chatGrain = _grainFactory.GetGrain<IChatGrain>(chatId);
        await chatGrain.InitializeAsync(request.UserId, request.Title);
        await chatGrain.AddMessageAsync(MessageType.User, request.Request, null, request.TokensUsed);

        var userGrain = _grainFactory.GetGrain<IUserGrain>(request.UserId);
        await userGrain.AddChatAsync(chatId);

        return new CreateNewChatResponse { ChatId = chatId };
    }
}
