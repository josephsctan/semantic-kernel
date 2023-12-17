using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Plugins;
using RepoUtils;

public static class ExampleF2_ACS_Rag
{
    public static async Task RunAsync()
    {
        string[] questions = new string[]
        {
            "What ISO 19650 certification does BECA have?",
            "What the fuck to you use for VR at BECA?",
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
        Kernel kernel = builder.Build();

        var acsPlugin = new ACSRagPlugin(ConsoleLogger.LoggerFactory);
        kernel.ImportPluginFromObject(acsPlugin, nameof(ACSRagPlugin));

        return kernel;

    }
}
