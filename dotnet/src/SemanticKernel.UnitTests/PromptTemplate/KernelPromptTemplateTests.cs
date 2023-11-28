﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using SemanticKernel.UnitTests.XunitHelpers;
using Xunit;
using Xunit.Abstractions;

namespace SemanticKernel.UnitTests.PromptTemplate;

public sealed class KernelPromptTemplateTests
{
    private const string DateFormat = "M/d/yyyy";
    private readonly KernelPromptTemplateFactory _factory;
    private readonly ContextVariables _variables;
    private readonly ITestOutputHelper _logger;
    private readonly Kernel _kernel;

    public KernelPromptTemplateTests(ITestOutputHelper testOutputHelper)
    {
        this._logger = testOutputHelper;
        this._factory = new KernelPromptTemplateFactory(TestConsoleLogger.LoggerFactory);
        this._variables = new ContextVariables(Guid.NewGuid().ToString("X"));
        this._kernel = new Kernel();
    }

    [Fact]
    public void ItRendersVariables()
    {
        // Arrange
        var template = "{$x11} This {$a} is {$_a} a {{$x11}} test {{$x11}} " +
                       "template {{foo}}{{bar $a}}{{baz $_a}}{{yay $x11}}{{food a='b' c = $d}}";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var blocks = target.ExtractBlocks(template);
        var updatedBlocks = target.RenderVariables(blocks, this._variables);

        // Assert
        Assert.Equal(10, blocks.Count);
        Assert.Equal(10, updatedBlocks.Count);

        Assert.Equal("$x11", blocks[1].Content);
        Assert.Equal("", updatedBlocks[1].Content);
        Assert.Equal(BlockTypes.Variable, blocks[1].Type);
        Assert.Equal(BlockTypes.Text, updatedBlocks[1].Type);

        Assert.Equal("$x11", blocks[3].Content);
        Assert.Equal("", updatedBlocks[3].Content);
        Assert.Equal(BlockTypes.Variable, blocks[3].Type);
        Assert.Equal(BlockTypes.Text, updatedBlocks[3].Type);

        Assert.Equal("foo", blocks[5].Content);
        Assert.Equal("foo", updatedBlocks[5].Content);
        Assert.Equal(BlockTypes.Code, blocks[5].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[5].Type);

        Assert.Equal("bar $a", blocks[6].Content);
        Assert.Equal("bar $a", updatedBlocks[6].Content);
        Assert.Equal(BlockTypes.Code, blocks[6].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[6].Type);

        Assert.Equal("baz $_a", blocks[7].Content);
        Assert.Equal("baz $_a", updatedBlocks[7].Content);
        Assert.Equal(BlockTypes.Code, blocks[7].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[7].Type);

        Assert.Equal("yay $x11", blocks[8].Content);
        Assert.Equal("yay $x11", updatedBlocks[8].Content);
        Assert.Equal(BlockTypes.Code, blocks[8].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[8].Type);

        Assert.Equal("food a='b' c = $d", blocks[9].Content);
        Assert.Equal("food a='b' c = $d", updatedBlocks[9].Content);
        Assert.Equal(BlockTypes.Code, blocks[9].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[9].Type);

        // Arrange
        this._variables.Set("x11", "x11 value");
        this._variables.Set("a", "a value");
        this._variables.Set("_a", "_a value");
        this._variables.Set("c", "c value");
        this._variables.Set("d", "d value");

        // Act
        blocks = target.ExtractBlocks(template);
        updatedBlocks = target.RenderVariables(blocks, this._variables);

        // Assert
        Assert.Equal(10, blocks.Count);
        Assert.Equal(10, updatedBlocks.Count);

        Assert.Equal("$x11", blocks[1].Content);
        Assert.Equal("x11 value", updatedBlocks[1].Content);
        Assert.Equal(BlockTypes.Variable, blocks[1].Type);
        Assert.Equal(BlockTypes.Text, updatedBlocks[1].Type);

        Assert.Equal("$x11", blocks[3].Content);
        Assert.Equal("x11 value", updatedBlocks[3].Content);
        Assert.Equal(BlockTypes.Variable, blocks[3].Type);
        Assert.Equal(BlockTypes.Text, updatedBlocks[3].Type);

        Assert.Equal("foo", blocks[5].Content);
        Assert.Equal("foo", updatedBlocks[5].Content);
        Assert.Equal(BlockTypes.Code, blocks[5].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[5].Type);

        Assert.Equal("bar $a", blocks[6].Content);
        Assert.Equal("bar $a", updatedBlocks[6].Content);
        Assert.Equal(BlockTypes.Code, blocks[6].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[6].Type);

        Assert.Equal("baz $_a", blocks[7].Content);
        Assert.Equal("baz $_a", updatedBlocks[7].Content);
        Assert.Equal(BlockTypes.Code, blocks[7].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[7].Type);

        Assert.Equal("yay $x11", blocks[8].Content);
        Assert.Equal("yay $x11", updatedBlocks[8].Content);
        Assert.Equal(BlockTypes.Code, blocks[8].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[8].Type);

        Assert.Equal("food a='b' c = $d", blocks[9].Content);
        Assert.Equal("food a='b' c = $d", updatedBlocks[9].Content);
        Assert.Equal(BlockTypes.Code, blocks[9].Type);
        Assert.Equal(BlockTypes.Code, updatedBlocks[9].Type);
    }

    [Fact]
    public async Task ItRendersCodeUsingInputAsync()
    {
        // Arrange
        string MyFunctionAsync(ContextVariables context)
        {
            this._logger.WriteLine("MyFunction call received, input: {0}", context.Input);
            return $"F({context.Input})";
        }

        var func = KernelFunctionFactory.CreateFromMethod(Method(MyFunctionAsync), this, "function");

        this._kernel.Plugins.Add(new KernelPlugin("plugin", new[] { func }));

        this._variables.Update("INPUT-BAR");
        var template = "foo-{{plugin.function}}-baz";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("foo-F(INPUT-BAR)-baz", result);
    }

    [Fact]
    public async Task ItRendersCodeUsingVariablesAsync()
    {
        // Arrange
        string MyFunctionAsync(ContextVariables context)
        {
            this._logger.WriteLine("MyFunction call received, input: {0}", context.Input);
            return $"F({context.Input})";
        }

        var func = KernelFunctionFactory.CreateFromMethod(Method(MyFunctionAsync), this, "function");

        this._kernel.Plugins.Add(new KernelPlugin("plugin", new[] { func }));

        this._variables.Set("myVar", "BAR");
        var template = "foo-{{plugin.function $myVar}}-baz";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("foo-F(BAR)-baz", result);
    }

    [Fact]
    public async Task ItRendersCodeUsingNamedVariablesAsync()
    {
        // Arrange
        string MyFunctionAsync(
            [Description("Name"), KernelName("input")] string name,
            [Description("Age"), KernelName("age")] int age,
            [Description("Slogan"), KernelName("slogan")] string slogan,
            [Description("Date"), KernelName("date")] DateTime date)
        {
            var dateStr = date.ToString(DateFormat, CultureInfo.InvariantCulture);
            this._logger.WriteLine("MyFunction call received, name: {0}, age: {1}, slogan: {2}, date: {3}", name, age, slogan, date);
            return $"[{dateStr}] {name} ({age}): \"{slogan}\"";
        }

        var func = KernelFunctionFactory.CreateFromMethod(Method(MyFunctionAsync), this, "function");

        this._kernel.Plugins.Add(new KernelPlugin("plugin", new[] { func }));

        this._variables.Set("input", "Mario");
        this._variables.Set("someDate", "2023-08-25T00:00:00");
        var template = "foo-{{plugin.function input=$input age='42' slogan='Let\\'s-a go!' date=$someDate}}-baz";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("foo-[8/25/2023] Mario (42): \"Let's-a go!\"-baz", result);
    }

    [Fact]
    public async Task ItHandlesSyntaxErrorsAsync()
    {
        this._variables.Set("input", "Mario");
        this._variables.Set("someDate", "2023-08-25T00:00:00");
        var template = "foo-{{function input=$input age=42 slogan='Let\\'s-a go!' date=$someDate}}-baz";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await Assert.ThrowsAsync<KernelException>(() => target.RenderAsync(this._kernel, this._variables));
        Assert.Equal($"Named argument values need to be prefixed with a quote or {Symbols.VarPrefix}.", result.Message);
    }

    [Fact]
    public async Task ItRendersCodeUsingImplicitInputAndNamedVariablesAsync()
    {
        // Arrange
        string MyFunctionAsync(
            [Description("Input"), KernelName("input")] string name,
            [Description("Age"), KernelName("age")] int age,
            [Description("Slogan"), KernelName("slogan")] string slogan,
            [Description("Date"), KernelName("date")] DateTime date)
        {
            this._logger.WriteLine("MyFunction call received, name: {0}, age: {1}, slogan: {2}, date: {3}", name, age, slogan, date);
            var dateStr = date.ToString(DateFormat, CultureInfo.InvariantCulture);
            return $"[{dateStr}] {name} ({age}): \"{slogan}\"";
        }

        KernelFunction func = KernelFunctionFactory.CreateFromMethod(Method(MyFunctionAsync), this, "function");

        this._kernel.Plugins.Add(new KernelPlugin("plugin", new[] { func }));

        this._variables.Set("input", "Mario");
        this._variables.Set("someDate", "2023-08-25T00:00:00");

        var template = "foo-{{plugin.function $input age='42' slogan='Let\\'s-a go!' date=$someDate}}-baz";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("foo-[8/25/2023] Mario (42): \"Let's-a go!\"-baz", result);
    }

    [Fact]
    public async Task ItRendersAsyncCodeUsingImmutableVariablesAsync()
    {
        // Arrange
        var template = "{{func1}} {{func2}} {{func3 $myVar}}";
        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));
        this._variables.Update("BAR");
        this._variables.Set("myVar", "BAZ");

        string MyFunction1Async(ContextVariables localVariables)
        {
            this._logger.WriteLine("MyFunction1 call received, input: {0}", localVariables.Input);
            localVariables.Update("foo");
            return "F(OUTPUT-FOO)";
        }
        string MyFunction2Async(ContextVariables localVariables)
        {
            // Input value should be "BAR" because the variable $input is immutable in MyFunction1
            this._logger.WriteLine("MyFunction2 call received, input: {0}", localVariables.Input);
            localVariables.Set("myVar", "bar");
            return localVariables.Input;
        }
        string MyFunction3Async(ContextVariables localVariables)
        {
            // Input value should be "BAZ" because the variable $myVar is immutable in MyFunction2
            this._logger.WriteLine("MyFunction3 call received, input: {0}", localVariables.Input);
            return localVariables.TryGetValue("myVar", out string? value) ? value : "";
        }

        var functions = new List<KernelFunction>()
        {
            KernelFunctionFactory.CreateFromMethod(Method(MyFunction1Async), this, "func1"),
            KernelFunctionFactory.CreateFromMethod(Method(MyFunction2Async), this, "func2"),
            KernelFunctionFactory.CreateFromMethod(Method(MyFunction3Async), this, "func3")
        };

        this._kernel.Plugins.Add(new KernelPlugin("plugin", functions));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("F(OUTPUT-FOO) BAR BAZ", result);
    }

    [Fact]
    public async Task ItRendersAsyncCodeUsingVariablesAsync()
    {
        // Arrange
        Task<string> MyFunctionAsync(ContextVariables localVariables)
        {
            // Input value should be "BAR" because the variable $myVar is passed in
            this._logger.WriteLine("MyFunction call received, input: {0}", localVariables.Input);
            return Task.FromResult(localVariables.Input);
        }

        KernelFunction func = KernelFunctionFactory.CreateFromMethod(Method(MyFunctionAsync), this, "function");

        this._kernel.Plugins.Add(new KernelPlugin("plugin", new[] { func }));

        this._variables.Set("myVar", "BAR");

        var template = "foo-{{plugin.function $myVar}}-baz";

        var target = (KernelPromptTemplate)this._factory.Create(new PromptTemplateConfig(template));

        // Act
        var result = await target.RenderAsync(this._kernel, this._variables);

        // Assert
        Assert.Equal("foo-BAR-baz", result);
    }

    private static MethodInfo Method(Delegate method)
    {
        return method.Method;
    }
}
