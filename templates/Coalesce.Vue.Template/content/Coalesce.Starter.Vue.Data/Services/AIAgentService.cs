using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Encodings.Web;
using System.Text.Json;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Coalesce.Starter.Vue.Data.Services;

[Coalesce, Service]
public class AIAgentService(
    IServiceProvider sp,
    IChatCompletionService ccs,
    IDataProtectionProvider dpp,
    ILogger<AIAgentService> logger)
{
    public record ChatResponse(string response, string history);

    /// <summary>
    /// A chat agent that directly uses all kernel plugin tools.
    /// </summary>
    [Coalesce]
    public async Task<ChatResponse> ChatAgent(
        string? history,
        string prompt,
        CancellationToken cancellationToken
    )
    {
        // Optional: Create different agents with different sets of plugins, prompts, etc.
        var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>()]);
        return await RunChatAgentAsync(kernel, history, prompt, cancellationToken);
    }

    private async Task<ChatResponse> RunChatAgentAsync(Kernel kernel, string? history, string prompt, CancellationToken cancellationToken)
    {
        // Define the agent
        ChatCompletionAgent agent = new()
        {
            Instructions = $"""
            The current date is {DateTimeOffset.Now}.
            Be concise in your responses, not overly wordy. Do not format with markdown.
            When listing data, gather subsequent pages using the page parameter. If you don't do this, state so in your response.
            """,
            Kernel = kernel,
            HistoryReducer = new ChatHistorySummarizationReducer(ccs, targetCount: 4, thresholdCount: 8),
            Arguments = new KernelArguments(new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };

        // Chat history is protected to prevent tampering
        var protector = dpp.CreateProtector(nameof(AIAgentService));
        ChatHistory chatHistory = string.IsNullOrWhiteSpace(history)
            ? new()
            : JsonSerializer.Deserialize<ChatHistory>(protector.Unprotect(history)) ?? new();

        // Snapshot the history before the request
        // so we can roll it back in case token limits are reached.
        ChatHistory preCompletionHistory = new(chatHistory);
        string result;
        try
        {
            await agent.ReduceAsync(chatHistory, cancellationToken);

            List<string> responses = [];
            var thread = new ChatHistoryAgentThread(chatHistory);
            await foreach (ChatMessageContent response in agent.InvokeAsync(prompt, thread))
            {
                responses.Add(response.ToString());
            }

            result = string.Join('\n', responses);
        }
        catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            logger.LogWarning(ex, "Unable to complete chat completion request.");
            result = "Chat services are currently busy, or your request is too complicated.";
            // Roll back history which may have just been populated with an overwhelming number of tokens.
            chatHistory = preCompletionHistory;
        }

        var newHistory = protector.Protect(JsonSerializer.Serialize(chatHistory, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }));

        return new(result, newHistory);
    }
}
