﻿using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Plugins;
using RepoUtils;

public static class ExampleF3_Random_Rag
{

    public static async Task RunAsync()
    {
        string[] questions = new string[]
        {
            "Was the eiffel tower supposed to be temporary?",
        };

        var kernel = InitializeKernel();

        var config = new FunctionCallingStepwisePlannerConfig
        {
            MaxIterations = 15,
            MaxTokens = 4000,
        };
        var planner = new FunctionCallingStepwisePlanner(config);

        foreach (var question in questions)
        {
            try
            {
                FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);

                Console.WriteLine($"Chat history:\n{result.ChatHistory?.AsJson()}");

                Console.WriteLine($"Q: {question}\nA: {result.FinalAnswer}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("Hit any key to exit");

        Console.ReadKey();
    }

    /// <summary>
    /// Initialize the kernel and load plugins.
    /// </summary>
    /// <returns>A kernel instance</returns>
    private static Kernel InitializeKernel()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey);
        var kernel = builder.Build();

        // add random answer 
        var rapPlugin = new RandomAnswerPlugin(ConsoleLogger.LoggerFactory);
        kernel.ImportPluginFromObject(rapPlugin, "RandomAnswerPlugin");

        return kernel;
    }
}
