﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.SemanticKernel.TemplateEngine.Handlebars;

internal class HandlebarsPromptTemplate : IPromptTemplate
{
    /// <summary>
    /// Constructor for PromptTemplate.
    /// </summary>
    /// <param name="promptConfig">Prompt template configuration</param>
    /// <param name="loggerFactory">Logger factory</param>
    public HandlebarsPromptTemplate(PromptTemplateConfig promptConfig, ILoggerFactory? loggerFactory = null)
    {
        this._loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        this._logger = this._loggerFactory.CreateLogger(typeof(HandlebarsPromptTemplate));
        this._promptModel = promptConfig;
        this._parameters = new(() => this.InitParameters());
    }

    /// <inheritdoc/>
    public async Task<string> RenderAsync(Kernel kernel, ContextVariables variables, CancellationToken cancellationToken = default)
    {
        var handlebars = HandlebarsDotNet.Handlebars.Create();

        foreach (IKernelPlugin plugin in kernel.Plugins)
        {
            foreach (KernelFunction function in plugin)
            {
                handlebars.RegisterHelper($"{plugin.Name}_{function.Name}", (writer, hcontext, parameters) =>
                {
                    var result = function.InvokeAsync(kernel, variables).GetAwaiter().GetResult();
                    writer.WriteSafeString(result.GetValue<string>());
                });
            }
        }

        var template = handlebars.Compile(this._promptModel.Template);

        var prompt = template(this.GetVariables(variables));

        return await Task.FromResult(prompt).ConfigureAwait(true);
    }

    #region private
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly PromptTemplateConfig _promptModel;
    private readonly Lazy<IReadOnlyList<KernelParameterMetadata>> _parameters;

    private List<KernelParameterMetadata> InitParameters()
    {
        List<KernelParameterMetadata> parameters = new(this._promptModel.InputParameters.Count);
        foreach (var p in this._promptModel.InputParameters)
        {
            parameters.Add(new KernelParameterMetadata(p.Name)
            {
                Description = p.Description,
                DefaultValue = p.DefaultValue
            });
        }

        return parameters;
    }

    private Dictionary<string, string> GetVariables(ContextVariables variables)
    {
        Dictionary<string, string> result = new();
        foreach (var p in this._promptModel.InputParameters)
        {
            if (!string.IsNullOrEmpty(p.DefaultValue))
            {
                result[p.Name] = p.DefaultValue;
            }
        }

        foreach (var kvp in variables)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    #endregion

}
