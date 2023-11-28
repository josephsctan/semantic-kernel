﻿// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;

namespace SemanticKernel.Experimental.Orchestration.Flow.IntegrationTests;

public sealed class SendEmailPlugin
{
    private static readonly JsonSerializerOptions s_writeIndented = new() { WriteIndented = true };

    [KernelFunction]
    [Description("Send email")]
    [KernelName("SendEmail")]
    public string SendEmail(
        [KernelName("email_address")] string emailAddress,
        [KernelName("answer")] string answer,
        ContextVariables variables)
    {
        var contract = new Email()
        {
            Address = emailAddress,
            Content = answer,
        };

        // for demo purpose only
        string emailPayload = JsonSerializer.Serialize(contract, s_writeIndented);
        variables["email"] = emailPayload;

        return "Here's the API contract I will post to mail server: " + emailPayload;
    }

    private sealed class Email
    {
        public string? Address { get; set; }

        public string? Content { get; set; }
    }
}
