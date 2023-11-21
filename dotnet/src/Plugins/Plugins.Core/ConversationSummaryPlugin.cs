﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Plugins.Core;

/// <summary>
/// Semantic plugin that enables conversations summarization.
/// </summary>
public class ConversationSummaryPlugin
{
    /// <summary>
    /// The max tokens to process in a single semantic function call.
    /// </summary>
    private const int MaxTokens = 1024;

    private readonly ISKFunction _summarizeConversationFunction;
    private readonly ISKFunction _conversationActionItemsFunction;
    private readonly ISKFunction _conversationTopicsFunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationSummaryPlugin"/> class.
    /// </summary>
    public ConversationSummaryPlugin()
    {
        AIRequestSettings settings = new()
        {
            ExtensionData = new()
            {
                { "Temperature", 0.1 },
                { "TopP", 0.5 },
                { "MaxTokens", MaxTokens }
            }
        };

        this._summarizeConversationFunction = SKFunction.FromPrompt(
            SemanticFunctionConstants.SummarizeConversationDefinition,
            description: "Given a section of a conversation transcript, summarize the part of the conversation.",
            requestSettings: settings);

        this._conversationActionItemsFunction = SKFunction.FromPrompt(
            SemanticFunctionConstants.GetConversationActionItemsDefinition,
            description: "Given a section of a conversation transcript, identify action items.",
            requestSettings: settings);

        this._conversationTopicsFunction = SKFunction.FromPrompt(
            SemanticFunctionConstants.GetConversationTopicsDefinition,
            description: "Analyze a conversation transcript and extract key topics worth remembering.",
            requestSettings: settings);
    }

    /// <summary>
    /// Given a long conversation transcript, summarize the conversation.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The kernel</param>
    /// <param name="context">The SKContext for function execution.</param>
    [SKFunction, Description("Given a long conversation transcript, summarize the conversation.")]
    public Task<string> SummarizeConversationAsync(
        [Description("A long conversation transcript.")] string input,
        Kernel kernel,
        SKContext context) =>
        ProcessAsync(this._summarizeConversationFunction, input, kernel, context);

    /// <summary>
    /// Given a long conversation transcript, identify action items.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The kernel.</param>
    /// <param name="context">The SKContext for function execution.</param>
    [SKFunction, Description("Given a long conversation transcript, identify action items.")]
    public Task<string> GetConversationActionItemsAsync(
        [Description("A long conversation transcript.")] string input,
        Kernel kernel,
        SKContext context) =>
        ProcessAsync(this._conversationActionItemsFunction, input, kernel, context);

    /// <summary>
    /// Given a long conversation transcript, identify topics.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The kernel.</param>
    /// <param name="context">The SKContext for function execution.</param>
    [SKFunction, Description("Given a long conversation transcript, identify topics worth remembering.")]
    public Task<string> GetConversationTopicsAsync(
        [Description("A long conversation transcript.")] string input,
        Kernel kernel,
        SKContext context) =>
        ProcessAsync(this._conversationTopicsFunction, input, kernel, context);

    private static async Task<string> ProcessAsync(ISKFunction func, string input, Kernel kernel, SKContext context)
    {
        List<string> lines = TextChunker.SplitPlainTextLines(input, MaxTokens);
        List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, MaxTokens);

        string[] results = new string[paragraphs.Count];
        for (int i = 0; i < results.Length; i++)
        {
            context.Variables.Update(paragraphs[i]);
            results[i] = (await func.InvokeAsync(kernel, context).ConfigureAwait(false)).GetValue<string>() ?? "";
        }

        return string.Join("\n", results);
    }
}
