﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.SemanticKernel.Planning.Action.UnitTests;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class ActionPlannerTests
{
    [Fact]
    public async Task ExtractsAndDeserializesWellFormedJsonFromPlannerResultAsync()
    {
        // Arrange
        var plugins = this.CreatePluginCollection();

        var kernel = this.CreateKernel(ValidPlanString, plugins);

        var planner = new ActionPlanner(kernel);

        // Act
        var plan = await planner.CreatePlanAsync("goal");

        // Assert
        Assert.Equal("goal", plan.Description);

        Assert.Single(plan.Steps);
        Assert.Equal("GitHubPlugin", plan.Steps[0].PluginName);
        Assert.Equal("PullsList", plan.Steps[0].Name);
    }

    [Fact]
    public async Task InvalidJsonThrowsAsync()
    {
        // Arrange
        string invalidJsonString = "<>";

        var kernel = this.CreateKernel(invalidJsonString);

        var planner = new ActionPlanner(kernel);

        // Act & Assert
        await Assert.ThrowsAsync<SKException>(() => planner.CreatePlanAsync("goal"));
    }

    [Fact]
    public void UsesPromptDelegateWhenProvided()
    {
        // Arrange
        var kernel = this.CreateKernel(string.Empty);

        var getPromptTemplateMock = new Mock<Func<string>>();

        var config = new ActionPlannerConfig()
        {
            GetPromptTemplate = getPromptTemplateMock.Object
        };

        // Act
        var planner = new ActionPlanner(kernel, config);

        // Assert
        getPromptTemplateMock.Verify(x => x(), Times.Once());
    }

    [Fact]
    public async Task MalformedJsonThrowsAsync()
    {
        // Arrange

        // Extra opening brace before rationale
        string invalidJsonString =
            @"Here is a possible plan to accomplish the user intent:
            {
                ""plan"": { {
                    ""rationale"": ""the list contains a function that allows to list pull requests"",
                    ""function"": ""GitHubPlugin.PullsList"",
                    ""parameters"": {
                        ""owner"": ""microsoft"",
                        ""repo"": ""semantic-kernel"",
                        ""state"": ""open""
                    }
                }
            }

            This plan uses the `GitHubPlugin.PullsList` function to list the open pull requests for the `semantic-kernel` repository owned by `microsoft`. The `state` parameter is set to `""open""` to filter the results to only show open pull requests.";

        var kernel = this.CreateKernel(invalidJsonString);

        var planner = new ActionPlanner(kernel);

        // Act & Assert
        await Assert.ThrowsAsync<SKException>(async () => await planner.CreatePlanAsync("goal"));
    }

    [Fact]
    public async Task ListOfFunctionsIncludesNativeAndPromptFunctionsAsync()
    {
        // Arrange
        var plugins = this.CreatePluginCollection();

        var kernel = this.CreateKernel(ValidPlanString, plugins);

        var planner = new ActionPlanner(kernel);

        var context = kernel.CreateNewContext();

        // Act
        var result = await planner.ListOfFunctionsAsync("goal", context);

        // Assert
        var expected = $"// Send an e-mail.{Environment.NewLine}email.SendEmail{Environment.NewLine}// List pull requests.{Environment.NewLine}GitHubPlugin.PullsList{Environment.NewLine}// List repositories.{Environment.NewLine}GitHubPlugin.RepoList{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ListOfFunctionsExcludesExcludedPluginsAsync()
    {
        // Arrange
        var plugins = this.CreatePluginCollection();

        var kernel = this.CreateKernel(ValidPlanString, plugins);

        var config = new ActionPlannerConfig();
        config.ExcludedPlugins.Add("GitHubPlugin");

        var planner = new ActionPlanner(kernel, config: config);

        var context = kernel.CreateNewContext();

        // Act
        var result = await planner.ListOfFunctionsAsync("goal", context);

        // Assert
        var expected = $"// Send an e-mail.{Environment.NewLine}email.SendEmail{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ListOfFunctionsExcludesExcludedFunctionsAsync()
    {
        // Arrange
        var plugins = this.CreatePluginCollection();

        var kernel = this.CreateKernel(ValidPlanString, plugins);

        var config = new ActionPlannerConfig();
        config.ExcludedFunctions.Add("PullsList");

        var planner = new ActionPlanner(kernel, config: config);

        var context = kernel.CreateNewContext();

        // Act
        var result = await planner.ListOfFunctionsAsync("goal", context);

        // Assert
        var expected = $"// Send an e-mail.{Environment.NewLine}email.SendEmail{Environment.NewLine}// List repositories.{Environment.NewLine}GitHubPlugin.RepoList{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    private Kernel CreateKernel(string testPlanString, SKPluginCollection? plugins = null)
    {
        plugins ??= new SKPluginCollection();

        var textResult = new Mock<ITextResult>();
        textResult
            .Setup(tr => tr.GetCompletionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testPlanString);

        var textCompletionResult = new List<ITextResult> { textResult.Object };

        var textCompletion = new Mock<ITextCompletion>();
        textCompletion
            .Setup(tc => tc.GetCompletionsAsync(It.IsAny<string>(), It.IsAny<AIRequestSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(textCompletionResult);

        var serviceSelector = new Mock<IAIServiceSelector>();
        serviceSelector
            .Setup(ss => ss.SelectAIService<ITextCompletion>(It.IsAny<SKContext>(), It.IsAny<ISKFunction>()))
            .Returns((textCompletion.Object, new AIRequestSettings()));

        var serviceProvider = new Mock<IAIServiceProvider>();

        return new Kernel(serviceProvider.Object, plugins, serviceSelector.Object);
    }

    private SKPluginCollection CreatePluginCollection()
    {
        return new()
        {
            new SKPlugin("email", new[]
            {
                SKFunction.FromMethod(() => "MOCK FUNCTION CALLED", "SendEmail", "Send an e-mail")
            }),
            new SKPlugin("GitHubPlugin", new[]
            {
                SKFunction.FromMethod(() => "MOCK FUNCTION CALLED", "PullsList", "List pull requests"),
                SKFunction.FromMethod(() => "MOCK FUNCTION CALLED", "RepoList", "List repositories")
            })
        };
    }

    private const string ValidPlanString =
        @"Here is a possible plan to accomplish the user intent:
        {
            ""plan"":{
                ""rationale"": ""the list contains a function that allows to list pull requests"",
                ""function"": ""GitHubPlugin.PullsList"",
                ""parameters"": {
                    ""owner"": ""microsoft"",
                    ""repo"": ""semantic-kernel"",
                    ""state"": ""open""
                }
            }
        }

        This plan uses the `GitHubPlugin.PullsList` function to list the open pull requests for the `semantic-kernel` repository owned by `microsoft`. The `state` parameter is set to `""open""` to filter the results to only show open pull requests.";
}
