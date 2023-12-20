using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1823 // Avoid unused private fields

namespace Frankly
{
    public class AzureCognitiveSearchSettings
    {
        public string IndexName { get; set; } = string.Empty;
        public string SemanticConfig { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string SelectFields { get; set; } = string.Empty;
    }

    public class AzureCognitiveSearchResults
    {
        public double Score { get; set; } = -1;
        public string Content { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Metadata { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interact with Azure Cognitive services 
    /// </summary>
    public class AzureCognitiveSearchHelper
    {

        AzureCognitiveSearchSettings? searchSettings;

        private int truncateResultsTo = 2000;
        private readonly IConfiguration _config;
        /// <summary>
        /// DI provides the configuration 
        /// </summary>
        /// <param name="configuration"></param>
        public AzureCognitiveSearchHelper()
        {

        }


        /// <summary>
        /// Vector search 
        /// </summary>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public Task<OperationResult<List<AzureCognitiveSearchResults>>> InvokeVectorSearchAsync(IReadOnlyList<float> vector, int numResults = 5)
        {
            return SearchAsync(true, null, vector, true, numResults);
        }

        /// <summary>
        /// hybrid search
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public Task<OperationResult<List<AzureCognitiveSearchResults>>> InvokeHybridSearchAsync(string term, IReadOnlyList<float> vector, int numResults = 5)
        {
            return SearchAsync(true, term, vector, true, numResults);
        }


        public Task<OperationResult<List<AzureCognitiveSearchResults>>> InvokeTextSearchAsync
            (string term, bool useSemanticRanking = false, int numResults = 5)
        {
            return SearchAsync(false, term, null, true, numResults);
        }

        /// <summary>
        /// one function to do it all 
        /// </summary>
        /// <param name="vectorSeach">if true perform vector search using the 'vector' param, else perform a search using the 
        /// search using the 'text' param</param>
        /// <param name="vector">used for sector seaech</param>
        /// <param name="term">used for text search</param>
        /// <param name="useSemanticRanking">if true, use semantic ordering</param>
        /// <param name="numResults">number of results to return (default 5)</param>
        /// <returns></returns>
        private async Task<OperationResult<List<AzureCognitiveSearchResults>>> SearchAsync(
            bool vectorSeach = false,
            string? term = null,
            IReadOnlyList<float>? vector = null,
            bool useSemanticRanking = false,
            int numResults = 5)
        {
            var resultsList = new List<AzureCognitiveSearchResults>();
            var retVal = OperationResult<List<AzureCognitiveSearchResults>>.NotOK();

            if (searchSettings == null)
            {
                return retVal.SetFail("Please configure Azure Cognitive Search in System Settings ");
            }

            try
            {
                List<string> stringList = new();

                // search vector to use
                var searchVector = vectorSeach ? vector : null;

                SearchOptions? options = ConstructSearchOptions(useSemanticRanking, searchVector);

                // Create a client
                Uri endpoint = new Uri(searchSettings.Endpoint);
                AzureKeyCredential credential = new AzureKeyCredential(searchSettings.Key);
                SearchClient client = new SearchClient(endpoint, searchSettings.IndexName, credential);

                // perform search 
                SearchResults<SearchDocument> response = await client.SearchAsync<SearchDocument>(term, options);

                // collect and return results 
                foreach (SearchResult<SearchDocument> result in response.GetResults().Take(numResults))
                {
                    resultsList.Add(ConvertResults(result));
                }
                retVal.SetOK(resultsList);
            }
            catch (Exception ex)
            {
                retVal.SetFail(ex.Message);
            }
            return retVal;
        }


        /// <summary>
        /// Create search options.  
        /// </summary>
        /// <param name="semanticConfig">If provided, the search options uses semantic ranking </param>
        /// <param name="vector">If provided, the search options included vector search </param>
        /// <returns></returns>
        private SearchOptions ConstructSearchOptions(bool useSemantic = true, IReadOnlyList<float>? vector = null)
        {
            string? semanticConfigName = useSemantic && !string.IsNullOrWhiteSpace(searchSettings?.SemanticConfig)
                    ? searchSettings.SemanticConfig : null;

            SearchOptions options = new SearchOptions()
            {
                QueryType = SearchQueryType.Simple,
            };

            // setup semantic rnaking 
            if (!string.IsNullOrWhiteSpace(semanticConfigName))
            {
                options.QueryType = SearchQueryType.Semantic;
                options.SemanticSearch = new SemanticSearchOptions()
                {
                    SemanticConfigurationName = semanticConfigName,
                    QueryCaption = new QueryCaption(QueryCaptionType.Extractive) { HighlightEnabled = true }
                };
            }

            // setup vector search
            if (vector != null)
            {
                options.VectorSearch = new VectorSearchOptions();

                options.VectorSearch.Queries.Add(new VectorizedQuery(vector.ToArray().AsMemory())
                {
                    KNearestNeighborsCount = 3,
                    Fields = { "content_vector" }
                });
            }

            // add other fields
            options.Select.Add("content");
            options.Select.Add("metadata");
            options.Select.Add("id");

            if (searchSettings != null)
            {
                // these must come from the settings
                foreach (var selectField in searchSettings.SelectFields.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    options.Select.Add(selectField);

                }
                // future:     "file_type|name|last_modified|web_url";
                // now:          "name|last_modified|webUrl";
            }

            return options;
        }

        private AzureCognitiveSearchResults ConvertResults(SearchResult<SearchDocument> resultDoc)
        {
            var acsResult = new AzureCognitiveSearchResults();

            SearchDocument doc = resultDoc.Document;
            acsResult.Score = resultDoc.Score ?? 0;
            object? theValue;
            if (doc.TryGetValue("metadata", out theValue))
            {
                acsResult.Metadata = theValue as string ?? string.Empty;
            }
            if (doc.TryGetValue("content", out theValue))
            {
                acsResult.Content = theValue as string ?? string.Empty;
            }
            if (doc.TryGetValue("file_type", out theValue))
            {
                acsResult.FileType = theValue as string ?? string.Empty;
            }

            // name or file_name
            if (doc.TryGetValue("name", out theValue))
            {
                acsResult.FileName = theValue as string ?? string.Empty;
            }
            else if (doc.TryGetValue("file_name", out theValue))
            {
                acsResult.FileName = theValue as string ?? string.Empty;
            }

            // web_url or webUrl 
            if (doc.TryGetValue("web_url", out theValue))
            {
                acsResult.WebUrl = theValue as string ?? string.Empty;
            }
            else if (doc.TryGetValue("webUrl", out theValue))
            {
                acsResult.WebUrl = theValue as string ?? string.Empty;
            }
            if (doc.TryGetValue("id", out theValue))
            {
                acsResult.Id = theValue as string ?? string.Empty;
            }
            return acsResult;
        }



    }



}
