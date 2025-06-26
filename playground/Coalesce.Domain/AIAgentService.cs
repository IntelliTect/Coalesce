using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Magentic;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Coalesce.Domain
{
    [Coalesce, Service]
    public class AIAgentService(IServiceProvider sp, IChatCompletionService ccs, IDataProtectionProvider dpp, ILogger<AIAgentService> logger)
    {
        public record ChatResponse(string response, string history);

        /// <summary>
        /// A chat agent that orchestrates other agents
        /// </summary>
        [Coalesce]
        public async Task<ChatResponse> OrchestratedAgent(
            string? history,
            string prompt,
            CancellationToken cancellationToken
        )
        {
            var kb2 = Kernel.CreateBuilder();
            kb2.Services.AddSingleton(ccs);
            var managerKernel = kb2.Build();

            StandardMagenticManager manager = new StandardMagenticManager(
                managerKernel.GetRequiredService<IChatCompletionService>(),
                new OpenAIPromptExecutionSettings())
            {
                MaximumInvocationCount = 5,
            };

            //GroupChatOrchestration orchestration = new GroupChatOrchestration(
            //    new RoundRobinGroupChatManager { MaximumInvocationCount = 5 },

            MagenticOrchestration orchestration = new MagenticOrchestration(
                manager,
                CreateAgent(
                    name: "Person",
                    description: "A helpful assistant that can provide information about, create, and update people",
                    instructions: StandardInstructions(),
                    plugins: ["Person"]
                ),
                CreateAgent(
                    name: "Company",
                    description: "A helpful assistant that can provide information about companies",
                    instructions: StandardInstructions(),
                    plugins: ["Company"]
                ),
                CreateAgent(
                    name: "Product",
                    description: "A helpful assistant that can provide information about products",
                    instructions: StandardInstructions(),
                    plugins: ["Product"]
                ))
            {
                ResponseCallback = async (message) => Console.WriteLine(message),
            };

            InProcessRuntime runtime = new InProcessRuntime();
            await runtime.StartAsync();

            string input = prompt;
            var result = await orchestration.InvokeAsync(input, runtime);

            string output = await result.GetValueAsync(TimeSpan.FromSeconds(30));

            await runtime.RunUntilIdleAsync();

            return new(output, "");
        }

        /// <summary>
        /// A chat agent that delegates to other chat completion services.
        /// </summary>
        [Coalesce]
        public async Task<ChatResponse> MetaCompletionAgent(
            string? history,
            string prompt,
            CancellationToken cancellationToken
        )
        {
            var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>().Where(kp => kp.Name is "AIAgentService")]);
            return await RunCompletion(
                history,
                $"""
                Continue interacting with the assistant tools you have been provided until the user's request is satisfactorily fulfilled. 
                The assistants have no memory, so re-provide all necessary context on each prompt.
                Always ask the assistants when fulfulling a request. Don't answer with data from your own training.
                {StandardInstructions()}
                """,
                prompt, kernel, cancellationToken);
        }

        /// <summary>
        /// A chat agent that directly uses all kernel plugin tools.
        /// </summary>
        [Coalesce]
        public async Task<ChatResponse> OmniToolAgent(
            string? history,
            string prompt,
            CancellationToken cancellationToken
        )
        {
            var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>()]);

            // Define the agent
            ChatCompletionAgent agent = new()
            {
                Instructions = $"""
                {StandardInstructions()}
                When listing data, gather subsequent pages using the page parameter. If you don't do this, state so in your response.
                """,
                Kernel = kernel,
                HistoryReducer = new ChatHistorySummarizationReducer(ccs, targetCount: 4, thresholdCount: 8),
                Arguments = new KernelArguments(new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
            };

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
                await agent.ReduceAsync(chatHistory);

                List<string> responses = [];
                var thread = new ChatHistoryAgentThread(chatHistory);
                await foreach (ChatMessageContent response in agent.InvokeAsync(prompt, thread))
                {
                    responses.Add(response.ToString());
                }

                // todo: detect which tools were called, return some kind of flag in the response if there might have been a mutation.
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

            //return await RunCompletion(
            //    history,
            //    $"""
            //    {StandardInstructions()}
            //    When listing data, gather subsequent pages using the page parameter. If you don't do this, state so in your response.
            //    """,
            //    prompt, kernel, cancellationToken);
        }

        private string StandardInstructions() =>
            $"""
            The current date is {DateTimeOffset.Now}.
            Be concise in your responses, not overly wordy. Do not format with markdown.
            """;

        [KernelPlugin("An assistant who works with people and companies.")]
        [Coalesce]
        public async Task<ChatResponse> PersonAgent(
            string prompt,
            CancellationToken cancellationToken
        )
        {
            var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>().Where(kp => kp.Name is "Person" or "Company")]);
            return await RunCompletion(null, "When listing data, gather subsequent pages using the page parameter. If you don't do this, state so in your response.",
                prompt, kernel, cancellationToken) with { history = "" };
        }

        [KernelPlugin("An assistant who works with products.")]
        [Coalesce]
        public async Task<ChatResponse> ProductAgent(
            string prompt,
            CancellationToken cancellationToken
        )
        {
            var kernel = new Kernel(sp, [.. sp.GetServices<KernelPlugin>().Where(kp => kp.Name is "Product")]);
            return await RunCompletion(null, "When listing data, gather subsequent pages using the page parameter. If you don't do this, state so in your response.",
                prompt, kernel, cancellationToken) with { history = "" };
        }


        private ChatCompletionAgent CreateAgent(string name, string description, string instructions, HashSet<string> plugins)
        {
            var kb = Kernel.CreateBuilder();
            kb.Services.AddSingleton(ccs);
            foreach (var item in sp.GetServices<KernelPlugin>().Where(kp => plugins.Contains(kp.Name)))
            {
                kb.Plugins.Add(item);
            }
            var kernel = kb.Build();

            // Define the agent
            ChatCompletionAgent agent =
                new()
                {
                    Name = name,
                    Instructions = instructions,
                    Description = description,
                    Kernel = kernel,
                    Arguments = new KernelArguments(new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    })
                };

            return agent;
        }

        private async Task<ChatResponse> RunCompletion(string? history, string system, string prompt, Kernel kernel, CancellationToken cancellationToken)
        {
            var protector = dpp.CreateProtector(nameof(AIAgentService));
            var previousHistory = string.IsNullOrWhiteSpace(history)
                ? new()
                : JsonSerializer.Deserialize<ChatHistory>(protector.Unprotect(history)) ?? new();

            var effectiveHistory = new ChatHistory();
            effectiveHistory.AddDeveloperMessage($"""
                The current date is {DateTimeOffset.Now}.
                Be concise in your responses, not overly wordy. Do not format with markdown.
                {system}
                """);
            effectiveHistory.AddRange(previousHistory.Where(message => message.Role != AuthorRole.System));
            effectiveHistory.AddUserMessage(prompt);

            var reducer = new ChatHistorySummarizationReducer(ccs, targetCount: 4, thresholdCount: 8);
            var reducedMessages = await reducer.ReduceAsync(effectiveHistory);
            if (reducedMessages is not null) effectiveHistory = new(reducedMessages);

            // Snapshot the history before the request
            // so we can roll it back in case token limits are reached.
            ChatHistory preCompletionHistory = new(effectiveHistory);
            string result;
            try
            {
                // Note: effectiveHistory will get populated with all function calls
                // that occur as part of completing the request. It won't get populated
                // with the assistant response message, though.
                var response = await ccs.GetChatMessageContentAsync(
                    effectiveHistory,
                    executionSettings: new()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    },
                    kernel: kernel,
                    cancellationToken);
                // todo: detect which tools were called, return some kind of flag in the response if there might have been a mutation.
                result = response.ToString();
            }
            catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning(ex, "Unable to complete chat completion request.");
                result = "Chat services are currently busy, or your request is too complicated.";
                // Roll back history which may have just been populated with an overwhelming number of tokens.
                effectiveHistory = preCompletionHistory;
            }

            effectiveHistory.AddAssistantMessage(result);
            var newHistory = protector.Protect(JsonSerializer.Serialize(effectiveHistory, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            return new(result, newHistory);
        }
    }
}
