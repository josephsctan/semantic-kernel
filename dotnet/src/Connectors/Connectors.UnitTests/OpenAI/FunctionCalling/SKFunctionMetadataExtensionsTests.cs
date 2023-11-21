﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;
using Xunit;

namespace SemanticKernel.Connectors.UnitTests.OpenAI.FunctionCalling;
public sealed class SKFunctionMetadataExtensionsTests
{
    [Fact]
    public void ItCanConvertToOpenAIFunctionNoParameters()
    {
        // Arrange
        var sut = new SKFunctionMetadata("foo")
        {
            PluginName = "bar",
            Description = "baz",
            ReturnParameter = new SKReturnParameterMetadata { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") },
        };

        // Act
        var result = sut.ToOpenAIFunction();

        // Assert
        Assert.Equal(sut.Name, result.FunctionName);
        Assert.Equal(sut.PluginName, result.PluginName);
        Assert.Equal(sut.Description, result.Description);
        Assert.Equal($"{sut.PluginName}_{sut.Name}", result.FullyQualifiedName);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equivalent(new OpenAIFunctionReturnParameter { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") }, result.ReturnParameter);
    }

    [Fact]
    public void ItCanConvertToOpenAIFunctionNoPluginName()
    {
        // Arrange
        var sut = new SKFunctionMetadata("foo")
        {
            PluginName = string.Empty,
            Description = "baz",
            ReturnParameter = new SKReturnParameterMetadata { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") },
        };

        // Act
        var result = sut.ToOpenAIFunction();

        // Assert
        Assert.Equal(sut.Name, result.FunctionName);
        Assert.Equal(sut.PluginName, result.PluginName);
        Assert.Equal(sut.Description, result.Description);
        Assert.Equal(sut.Name, result.FullyQualifiedName);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equivalent(new OpenAIFunctionReturnParameter { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") }, result.ReturnParameter);
    }

    [Fact]
    public void ItCanConvertToOpenAIFunctionWithParameter()
    {
        // Arrange
        var param1 = new SKParameterMetadata("param1")
        {
            Description = "This is param1",
            DefaultValue = "1",
            Type = new ParameterJsonType("int"),
            ParameterType = typeof(int),
            IsRequired = false
        };

        var sut = new SKFunctionMetadata("foo")
        {
            PluginName = "bar",
            Description = "baz",
            Parameters = new[] { param1 },
            ReturnParameter = new SKReturnParameterMetadata { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") },
        };

        // Act
        var result = sut.ToOpenAIFunction();
        var outputParam = result.Parameters.First();

        // Assert
        Assert.Equal("int", outputParam.Type);
        Assert.Equal(param1.Name, outputParam.Name);
        Assert.Equal("This is param1 (default value: 1)", outputParam.Description);
        Assert.Equal(param1.IsRequired, outputParam.IsRequired);
        Assert.NotNull(outputParam.Schema);
        Assert.Equal("integer", outputParam.Schema.RootElement.GetProperty("type").GetString());
        Assert.Equivalent(new OpenAIFunctionReturnParameter { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") }, result.ReturnParameter);
    }

    [Fact]
    public void ItCanConvertToOpenAIFunctionWithParameterNoType()
    {
        // Arrange
        var param1 = new SKParameterMetadata("param1")
        {
            Description = "This is param1"
        };

        var sut = new SKFunctionMetadata("foo")
        {
            PluginName = "bar",
            Description = "baz",
            Parameters = new[] { param1 },
            ReturnParameter = new SKReturnParameterMetadata { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") },
        };

        // Act
        var result = sut.ToOpenAIFunction();
        var outputParam = result.Parameters.First();

        // Assert
        Assert.Equal("string", outputParam.Type);
        Assert.Equal(param1.Name, outputParam.Name);
        Assert.Equal(param1.Description, outputParam.Description);
        Assert.Equal(param1.IsRequired, outputParam.IsRequired);
        Assert.Equivalent(new OpenAIFunctionReturnParameter { Description = "retDesc", Schema = SKJsonSchema.Parse("\"schema\"") }, result.ReturnParameter);
    }

    [Fact]
    public void ItCanConvertToOpenAIFunctionWithNoReturnParameterType()
    {
        // Arrange
        var param1 = new SKParameterMetadata("param1")
        {
            Description = "This is param1",
            ParameterType = typeof(int),
        };

        var sut = new SKFunctionMetadata("foo")
        {
            PluginName = "bar",
            Description = "baz",
            Parameters = new[] { param1 },
        };

        // Act
        var result = sut.ToOpenAIFunction();
        var outputParam = result.Parameters.First();

        // Assert
        Assert.Equal("string", outputParam.Type);
        Assert.Equal(param1.Name, outputParam.Name);
        Assert.Equal(param1.Description, outputParam.Description);
        Assert.Equal(param1.IsRequired, outputParam.IsRequired);
        Assert.NotNull(outputParam.Schema);
        Assert.Equal("integer", outputParam.Schema.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void ItCanCreateValidOpenAIFunctionManual()
    {
        // Arrange
        var kernel = new KernelBuilder().Build();

        var plugin = kernel.ImportPluginFromObject(new MyPlugin(), "MyPlugin");

        var function = plugin.First();

        var functionView = function.GetMetadata();

        var sut = functionView.ToOpenAIFunction();

        // Act
        var result = sut.ToFunctionDefinition();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "{\"type\":\"object\",\"required\":[\"parameter1\",\"parameter2\",\"parameter3\"],\"properties\":{\"parameter1\":{\"type\":\"string\",\"description\":\"String parameter\"},\"parameter2\":{\"enum\":[\"Value1\",\"Value2\"],\"description\":\"Enum parameter\"},\"parameter3\":{\"type\":\"string\",\"format\":\"date-time\",\"description\":\"DateTime parameter\"}}}",
            result.Parameters.ToString()
        );
    }

    private enum MyEnum
    {
        Value1,
        Value2
    }

    private sealed class MyPlugin
    {
        [SKFunction, SKName("MyFunction"), System.ComponentModel.Description("My sample function.")]
        public string MyFunction(
            [System.ComponentModel.Description("String parameter")] string parameter1,
            [System.ComponentModel.Description("Enum parameter")] MyEnum parameter2,
            [System.ComponentModel.Description("DateTime parameter")] DateTime parameter3
            )
        {
            return "return";
        }
    }
}
