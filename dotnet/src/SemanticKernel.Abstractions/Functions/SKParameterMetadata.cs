﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace - Using the main namespace
namespace Microsoft.SemanticKernel;
#pragma warning restore IDE0130

/// <summary>
/// Provides read-only metadata for an <see cref="ISKFunction"/> parameter.
/// </summary>
public sealed class SKParameterMetadata
{
    /// <summary>The name of the parameter.</summary>
    private string _name = string.Empty;
    /// <summary>The description of the parameter.</summary>
    private string _description = string.Empty;

    /// <summary>Initializes the <see cref="SKParameterMetadata"/> for a parameter with the specified name.</summary>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> was null.</exception>
    public SKParameterMetadata(string name)
    {
        this.Name = name;
    }

    /// <summary>Initializes a <see cref="SKParameterMetadata"/> as a copy of another <see cref="SKParameterMetadata"/>.</summary>
    /// <exception cref="ArgumentNullException">The <paramref name="metadata"/> was null.</exception>
    /// <remarks>This creates a shallow clone of <paramref name="metadata"/>.</remarks>
    public SKParameterMetadata(SKParameterMetadata metadata)
    {
        Verify.NotNull(metadata);
        this.Name = metadata.Name;
        this.Description = metadata.Description;
        this.DefaultValue = metadata.DefaultValue;
        this.IsRequired = metadata.IsRequired;
        this.ParameterType = metadata.ParameterType;
        this.Type = metadata.Type;
        this.Schema = metadata.Schema;
    }

    /// <summary>Gets the name of the function.</summary>
    public string Name
    {
        get => this._name;
        init
        {
            Verify.NotNull(value);
            this._name = value;
        }
    }

    /// <summary>Gets a description of the function, suitable for use in describing the purpose to a model.</summary>
    [AllowNull]
    public string Description
    {
        get => this._description;
        init => this._description = value ?? string.Empty;
    }

    /// <summary>Gets the default value of the parameter.</summary>
    public string? DefaultValue { get; init; }

    /// <summary>Gets whether the parameter is required.</summary>
    public bool IsRequired { get; init; }

    /// <summary>Gets the .NET type of the parameter.</summary>
    public Type? ParameterType { get; init; }

    /// <summary>Gets the JSON type of the parameter.</summary>
    public ParameterJsonType? Type { get; init; }

    /// <summary>Gets a JSON Schema describing the parameter's type.</summary>
    public SKJsonSchema? Schema { get; init; }
}
