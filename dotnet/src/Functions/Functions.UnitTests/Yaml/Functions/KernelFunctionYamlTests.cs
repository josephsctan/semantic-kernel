﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Functions.Yaml.Functions;
using Xunit;

namespace SemanticKernel.Functions.UnitTests.Yaml.Functions;

public class KernelFunctionYamlTests
{
    [Fact]
    public void ItShouldCreateFunctionFromPromptYamlWithNoModelSettings()
    {
        // Arrange
        // Act
        var function = KernelFunctionYaml.FromPromptYaml(this._yamlNoModelSettings);

        // Assert
        Assert.NotNull(function);
        Assert.Equal("SayHello", function.Name);
        Assert.Equal("Say hello to the specified person using the specified language", function.Description);
        Assert.Equal(2, function.Metadata.Parameters.Count);
        //Assert.Equal(0, function.ExecutionSettings.Count);
    }

    [Fact]
    public void ItShouldCreateFunctionFromPromptYaml()
    {
        // Arrange
        // Act
        var function = KernelFunctionYaml.FromPromptYaml(this._yaml);

        // Assert
        Assert.NotNull(function);
        Assert.Equal("SayHello", function.Name);
        Assert.Equal("Say hello to the specified person using the specified language", function.Description);
    }

    [Fact]
    public void ItShouldCreateFunctionFromPromptYamlWithCustomModelSettings()
    {
        // Arrange
        // Act
        var function = KernelFunctionYaml.FromPromptYaml(this._yamlWithCustomSettings);

        // Assert
        Assert.NotNull(function);
        Assert.Equal("SayHello", function.Name);
        Assert.Equal("Say hello to the specified person using the specified language", function.Description);
        Assert.Equal(2, function.Metadata.Parameters.Count);
    }

    private readonly string _yamlNoModelSettings = @"
    template_format: semantic-kernel
    template:        Say hello world to {{$name}} in {{$language}}
    description:     Say hello to the specified person using the specified language
    name:            SayHello
    input_parameters:
      - name:          name
        description:   The name of the person to greet
        default_value: John
      - name:          language
        description:   The language to generate the greeting in
        default_value: English
";

    private readonly string _yaml = @"
    template_format: semantic-kernel
    template:        Say hello world to {{$name}} in {{$language}}
    description:     Say hello to the specified person using the specified language
    name:            SayHello
    input_parameters:
      - name:          name
        description:   The name of the person to greet
        default_value: John
      - name:          language
        description:   The language to generate the greeting in
        default_value: English
    execution_settings:
      - model_id:          gpt-4
        temperature:       1.0
        top_p:             0.0
        presence_penalty:  0.0
        frequency_penalty: 0.0
        max_tokens:        256
        stop_sequences:    []
      - model_id:          gpt-3.5
        temperature:       1.0
        top_p:             0.0
        presence_penalty:  0.0
        frequency_penalty: 0.0
        max_tokens:        256
        stop_sequences:    [ ""foo"", ""bar"", ""baz"" ]
";

    private readonly string _yamlWithCustomSettings = @"
    template_format: semantic-kernel
    template:        Say hello world to {{$name}} in {{$language}}
    description:     Say hello to the specified person using the specified language
    name:            SayHello
    input_parameters:
      - name:          name
        description:   The name of the person to greet
        default_value: John
      - name:          language
        description:   The language to generate the greeting in
        default_value: English
    execution_settings:
      - model_id:          gpt-4
        temperature:       1.0
        top_p:             0.0
        presence_penalty:  0.0
        frequency_penalty: 0.0
        max_tokens:        256
        stop_sequences:    []
      - model_id:          random-model
        temperaturex:      1.0
        top_q:             0.0
        rando_penalty:     0.0
        max_token_count:   256
        stop_sequences:    [ ""foo"", ""bar"", ""baz"" ]
";
}
