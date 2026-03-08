using MediatR;
using MijnCopilot.Agents;
using MijnCopilot.Agents.Model;
using MijnCopilot.Contracts.Grains;
using MijnCopilot.Model;

namespace MijnCopilot.Application.Agents.Commands;

public class ChatCommand : IRequest<ChatResponse>
{
    public Guid ChatId { get; set; }
    public string Request { get; set; }
    public bool IgnoreRequest { get; set; }
}

public class ChatResponse
{
    public string Response { get; set; }
}

public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResponse>
{
    private readonly IGrainFactory _grainFactory;
    private readonly ICopilotHelper _copilotHelper;

    public ChatCommandHandler(IGrainFactory grainFactory, ICopilotHelper copilotHelper)
    {
        _grainFactory = grainFactory;
        _copilotHelper = copilotHelper;
    }

    public async Task<ChatResponse> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        var chatGrain = _grainFactory.GetGrain<IChatGrain>(request.ChatId);
        var history = await chatGrain.GetHistoryAsync();

        var chatHistory = new CopilotChatHistory();
        foreach (var message in history)
        {
            switch (message.Type)
            {
                case MessageType.User:
                    chatHistory.AddUserMessage(message.Content);
                    break;
                case MessageType.Assistant:
                    chatHistory.AddAssistantMessage(message.Content);
                    break;
            }
        }

        if (!request.IgnoreRequest)
        {
            chatHistory.AddUserMessage(request.Request);
        }

        var copilotResponse = await _copilotHelper.Chat(chatHistory);

        if (!request.IgnoreRequest)
        {
            await chatGrain.AddMessageAsync(MessageType.User, request.Request, string.Empty, copilotResponse.InputTokenCount);
        }

        foreach (var debugMessage in copilotResponse.Debug)
        {
            await chatGrain.AddMessageAsync(
                debugMessage.IsQuestion ? MessageType.DebugQuestion : MessageType.DebugAnswer,
                debugMessage.Content,
                debugMessage.AgentName,
                0);
        }

        await chatGrain.AddMessageAsync(
            MessageType.Assistant,
            copilotResponse.LastAssistantMessage,
            copilotResponse.AgentName,
            copilotResponse.OutputTokenCount);

        return new ChatResponse { Response = copilotResponse.LastAssistantMessage };
    }
}