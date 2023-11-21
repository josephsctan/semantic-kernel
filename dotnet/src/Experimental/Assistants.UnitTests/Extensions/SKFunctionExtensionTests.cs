﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Assistants.Extensions;
using Moq;
using Xunit;

namespace SemanticKernel.Experimental.Assistants.UnitTests.Extensions;

[Trait("Category", "Unit Tests")]
[Trait("Feature", "Assistant")]
public sealed class SKFunctionExtensionTests
{
    private const string ToolName = "Bogus";
    private const string PluginName = "Fake";

    [Fact]
    public static void GetTwoPartName()
    {
        var mockFunction = new Mock<ISKFunction>();
        mockFunction.SetupGet(f => f.Name).Returns(ToolName);

        string qualifiedName = mockFunction.Object.GetQualifiedName(PluginName);

        Assert.Equal($"{PluginName}-{ToolName}", qualifiedName);
    }

    [Fact]
    public static void GetToolModelFromFunction()
    {
        const string FunctionDescription = "Bogus description";
        const string RequiredParamName = "required";
        const string OptionalParamName = "optional";

        var requiredParam = new SKParameterMetadata("required") { IsRequired = true };
        var optionalParam = new SKParameterMetadata("optional");
        var parameters = new List<SKParameterMetadata> { requiredParam, optionalParam };
        var functionView = new SKFunctionMetadata(ToolName)
        {
            PluginName = PluginName,
            Description = FunctionDescription,
            Parameters = parameters
        };

        var mockFunction = new Mock<ISKFunction>();
        mockFunction.SetupGet(f => f.Name).Returns(ToolName);
        mockFunction.SetupGet(f => f.Description).Returns(FunctionDescription);
        mockFunction.Setup(f => f.GetMetadata()).Returns(functionView);

        var toolModel = mockFunction.Object.ToToolModel(PluginName);
        var properties = toolModel.Function?.Parameters.Properties;
        var required = toolModel.Function?.Parameters.Required;

        Assert.Equal("function", toolModel.Type);
        Assert.Equal($"{PluginName}-{ToolName}", toolModel.Function?.Name);
        Assert.Equal(FunctionDescription, toolModel.Function?.Description);
        Assert.Equal(2, properties?.Count);
        Assert.True(properties?.ContainsKey(RequiredParamName));
        Assert.True(properties?.ContainsKey(OptionalParamName));
        Assert.Equal(1, required?.Count ?? 0);
        Assert.True(required?.Contains(RequiredParamName) ?? false);
    }
}
