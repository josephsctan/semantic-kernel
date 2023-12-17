using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.PromptTemplate.Handlebars;
using NCalc.Domain;
using RepoUtils;

public static class ExampleF4_Import_Yaml
{
    const string promptYaml = @"
name: TestPrompt
template_format: semantic-kernel
description: Just a test prompt 
input_variables:
  - name: input
    description: Just an input 
    is_required: true
template: |
    Give me the first letter in this input: {{$input}}
";


    const string promptYamlHandlebars = @"
name: TestPrompt
template_format: handlebars
description: Just a test prompt 
input_variables:
  - name: input
    description: Just an input 
    is_required: true
template: |
    Give me the first letter in this input: {{input}}
";
    public static async Task RunAsync()
    {
        Kernel kernel = new KernelBuilder()
            //.WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.ChatDeploymentName,
                TestConfiguration.AzureOpenAI.ChatModelId,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();


        await LoadHandlebarsYaml(kernel);
        return;

        KernelFunction function = kernel.CreateFunctionFromPromptYaml(promptYaml);
        var p1 = function?.Metadata?.Parameters?.FirstOrDefault();
        Console.WriteLine($" {p1?.Name} isrequired={p1?.IsRequired}");
        Console.WriteLine($" Hit any key to exit");
        Console.ReadKey();
        //// Load prompt from YAML
        //var destDir = "C:\\temp\\ExtractCitations.yaml";
        //var citationPrompt = File.ReadAllText(destDir);
        //var function = kernel.CreateFunctionFromPromptYaml(citationPrompt);
        //KernelPlugin plugin = new("aplugin");
        //plugin.AddFunction(function);

        //return kernel;
    }

    private static async Task LoadHandlebarsYaml(Kernel kernel)
    {
        KernelFunction function = kernel.CreateFunctionFromPromptYaml(
            promptYamlHandlebars,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory()
        );
        var p1 = function?.Metadata?.Parameters?.FirstOrDefault();

        Console.WriteLine($" {p1?.Name} isrequired={p1?.IsRequired}");
        var result = await kernel.InvokeAsync(
            function,
            arguments: new() {
                { "input", "This is a sentence"}
            });
        Console.WriteLine($" Result = {result.ToString()}");
        Console.WriteLine($" Hit any key to exit");
        Console.ReadKey();
    }
}
