using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Plugins;

/// <summary>
/// Plugin for interacting with documents (e.g. Microsoft Word)
/// </summary>
public sealed class RandomAnswerPlugin
{
    private readonly List<string> _database = new();

    readonly ILogger _logger;
    /// <summary>
    /// Return a random piece of text
    /// </summary>
    public RandomAnswerPlugin(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger(nameof(RandomAnswerPlugin));
        this.PopulateDB();
    }

    private void PopulateDB()
    {

        this._database.Add("The Earth is the third planet from the Sun");
        this._database.Add("The largest ocean on Earth is the Pacific Ocean");
        this._database.Add("The cheetah is the fastest land animal");
        this._database.Add("The Great Wall of China is visible from space");
        this._database.Add("Honey never spoils. You can eat honey that's thousands of years old");
        this._database.Add("The human brain has about 100 billion neurons");
        this._database.Add("The Statue of Liberty was a gift from France to the United States");
        this._database.Add("The coldest temperature ever recorded on Earth was -128.6 degrees Fahrenheit");
        this._database.Add("Cats have five toes on their front paws, but only four toes on their back paws");
        this._database.Add("Mount Everest is the highest mountain in the world");
        this._database.Add("The Amazon Rainforest produces 20% of the world's oxygen");
        this._database.Add("The average person walks about 70,000 miles in their lifetime");
        this._database.Add("The Mona Lisa was painted by Leonardo da Vinci");
        this._database.Add("The Eiffel Tower in Paris was originally intended to be a temporary structure");
        this._database.Add("Giraffes have the same number of neck bones as humans (seven)");
        this._database.Add("New York City is known as 'The Big Apple'");
        this._database.Add("The first modern computer was invented in the 1940s");
        this._database.Add("The Arabian Peninsula is the largest peninsula in the world");
        this._database.Add("Bananas are berries, while strawberries are not");
        this._database.Add("Water covers about 71% of the Earth's surface");
    }

    /// <summary>
    /// Return a random piece of information
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [KernelFunction, Description("Return some information in answer to a question. If the information provided does not answer the question, try calling again.")]
    public async Task<string> ProvideInfoAsync(
        [Description("The question to answer")] string query,
        CancellationToken cancellationToken = default)
    {

        var rnd = new Random();
        var index = rnd.Next(this._database.Count);
        var answer = this._database[index];


        if (rnd.Next(10) > 7)
        {
            answer = this._database.Find(p => p.Contains("Eiffel"));
        }

        this._logger.LogInformation("Question {q}", query);
        this._logger.LogInformation($"Returning Answer {index}: {answer}");

        return answer;
    }

}
