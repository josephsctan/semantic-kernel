﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.SemanticKernel.Events;

/// <summary>
/// Event arguments available to the Kernel.PromptRendering event.
/// </summary>
public class PromptRenderingEventArgs : KernelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PromptRenderingEventArgs"/> class.
    /// </summary>
    /// <param name="function">Kernel function</param>
    /// <param name="variables">Context variables related to the event</param>
    /// <param name="executionSettings">request settings used by the AI service</param>
    public PromptRenderingEventArgs(KernelFunction function, ContextVariables variables, PromptExecutionSettings? executionSettings) : base(function, variables)
    {
        this.RequestSettings = executionSettings; // TODO clone these settings
    }

    /// <summary>
    /// Request settings for the AI service.
    /// </summary>
    public PromptExecutionSettings? RequestSettings { get; }
}
