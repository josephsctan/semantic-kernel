using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace Frankly
{
    public class AzureCognitiveSearchSettings
    {
        public string IndexName { get; set; } = string.Empty;
        public string SemanticConfig { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interact with Azure Cognitive services 
    /// </summary>
    public class AzureCognitiveSearchHelper
    {
        AzureCognitiveSearchSettings searchSettings = new AzureCognitiveSearchSettings()
        {
            IndexName = "dd-sharepoint-custom-index2",
            SemanticConfig = "dd-semantic-config",
            Endpoint = "https://frankly-cognitive-search-basic.search.windows.net",
            Key = "g1Nir0MToV9jRcefqJLxlrgoimHRYlqfT0bqddkS2TAzSeCYJSwU"
        };

        private int truncateResultsTo = 2000;

        public AzureCognitiveSearchHelper()
        {
        }

        public async Task<OperationResult<object>> InvokeSearchAsync(string term, bool semantic = false, int numResults = 5)
        {
            var retVal = OperationResult<object>.NotOK();
            if (searchSettings == null)
            {
                return retVal.SetFail("Please configure Azure Cognitive Search in System Settings ");
            }

            try
            {
                List<string> stringList = new();

                SearchOptions? options = null;

                if (semantic && !string.IsNullOrWhiteSpace(searchSettings.SemanticConfig))
                {
                    options = new SearchOptions()
                    {
                        QueryType = SearchQueryType.Semantic,
                        QueryLanguage = QueryLanguage.EnUs,
                        SemanticConfigurationName = searchSettings.SemanticConfig,
                        QueryCaption = QueryCaptionType.Extractive,
                        QueryCaptionHighlightEnabled = true
                    };

                    options.Select.Add("content");
                    options.Select.Add("metadata");
                    //options.VectorQueries = new[] { };
                    options.Select.Add("id");
                }

                Uri endpoint = new Uri(searchSettings.Endpoint);

                // Create a client
                AzureKeyCredential credential = new AzureKeyCredential(searchSettings.Key);
                SearchClient client = new SearchClient(endpoint, searchSettings.IndexName, credential);

                SearchResults<SearchDocument> response = await client.SearchAsync<SearchDocument>(term, options);
                foreach (SearchResult<SearchDocument> result in response.GetResults().Take(numResults))
                {
                    SearchDocument doc = result.Document;


                    var score = result.Score ?? 0;
                    var metdata = doc["metadata"];
                    var content = doc["content"] as string;
                    string entry = $"<div>Score = {score} <code>{metdata}</code>" +
                        $"<br/> {content?.TruncateTo(truncateResultsTo, true)}</div>";
                    stringList.Add(entry);
                }
                retVal.SetOK(stringList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                retVal.SetFail(ex.Message);
            }
            return retVal;
        }

        /// <summary>
        /// Vector search 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public async Task<OperationResult<object>> InvokeVectorSearchAsync(IReadOnlyList<float> vector,
            int numResults = 5)
        {
            var retVal = OperationResult<object>.NotOK();

            if (searchSettings == null)
            {
                return retVal.SetFail("Please configure Azure Cognitive Search in System Settings ");
            }

            try
            {
                List<string> stringList = new();

                SearchOptions? options = null;

                options = new SearchOptions()
                {
                    QueryType = SearchQueryType.Simple,
                    QueryLanguage = QueryLanguage.EnUs,
                };

                if (!string.IsNullOrWhiteSpace(searchSettings.SemanticConfig))
                {
                    options.QueryType = SearchQueryType.Semantic;
                    options.SemanticConfigurationName = searchSettings.SemanticConfig;
                    options.QueryCaption = QueryCaptionType.Extractive;
                    options.QueryCaptionHighlightEnabled = true;
                }

                options.VectorQueries.Add(new RawVectorQuery()
                {
                    KNearestNeighborsCount = 3,
                    Fields = { "content_vector" },
                    // Vector = vector.ToList().AsReadOnly()
                    Vector = vector
                });

                options.Select.Add("content");
                options.Select.Add("metadata");
                options.Select.Add("id");

                Uri endpoint = new Uri(searchSettings.Endpoint);

                // Create a client
                AzureKeyCredential credential = new AzureKeyCredential(searchSettings.Key);
                SearchClient client = new SearchClient(endpoint, searchSettings.IndexName, credential);

                SearchResults<SearchDocument> response = await client.SearchAsync<SearchDocument>(null, options);

                await foreach (SearchResult<SearchDocument> result in response.GetResultsAsync())
                {
                    SearchDocument doc = result.Document;

                    var score = result.Score ?? 0;
                    var metdata = doc["metadata"];
                    var content = doc["content"] as string;
                    string entry = $"<div>Score = {score} <code>{metdata}</code>" +
                        $"<br/> {content?.TruncateTo(truncateResultsTo, true)}</div>";
                    stringList.Add(entry);

                    if (--numResults <= 0) // equivalent of .Take(numResults)
                        break;
                }
                retVal.SetOK(stringList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                retVal.SetFail(ex.Message);
            }
            return retVal;
        }
    }
}
