using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frankly;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Plugins;

/// <summary>
/// Plugin for interacting with Azure cognitive search
/// </summary>
public sealed class ACSRagPlugin
{
    readonly ILogger _logger;
    /// <summary>
    /// Return a random piece of text
    /// </summary>
    public ACSRagPlugin(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger(nameof(ACSRagPlugin));
    }
    /// <summary>
    /// Return a random piece of information
    /// </summary>
    [KernelFunction, Description("Search my knowledge base to return information to answer the user's question. " +
        "If the results I return do not answer the question well, try calling again.")]
    public async Task<string> AzureCognitiveSearchAsync(
        [Description("The question to answer")] string query,
        CancellationToken cancellationToken = default)
    {
        string returnString = "";
        var ACSH = new AzureCognitiveSearchHelper();
        var result = await ACSH.InvokeTextSearchAsync(query, true, 3);
        if (result.OK)
        {
            var textResults = result.Result.Select(p => p.Content).ToList();

            if (result.Result?.GetType() == typeof(List<string>))
            {
                returnString = string.Join("\n", textResults);
            }
            else
            {
                returnString = result.Result!.ToString();
            }
            returnString = "# ANSWER FROM KNOWLEDGE BASE:\n" + returnString;
        }
        else
        {
            returnString = result.ErrorMessage!;
        }

        _logger.LogInformation("Question: {q}\nAnswer: {a}\n", query, returnString);
        return returnString;
    }
}
