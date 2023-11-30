using System;
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
        var planner = new FunctionCallingStepwisePlanner(kernel, config);

        foreach (var question in questions)
        {
            try
            {
                FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(question);

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
        Kernel kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.ChatDeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        // add random answer 
        var rapPlugin = new RandomAnswerPlugin(ConsoleLogger.LoggerFactory);
        kernel.ImportPluginFromObject(rapPlugin, "RandomAnswerPlugin");

        return kernel;
    }
}
