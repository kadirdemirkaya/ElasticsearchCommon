using ElasticSearchCommon.Settings;
using Nest;

namespace ElasticSearchCommon.Services;

public class ElasticsearchService<T> : IElasticsearchService<T> where T : class
{
    private readonly IElasticClient _elasticClient;
    private readonly IElasticClient _elasticConfClient;
    private static string indexName = typeof(T).Name.ToLower();

    public ElasticsearchService(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
        _elasticConfClient = GetElasticConnection();
    }

    public async Task<IElasticClient> GetConnection(ElasticConfiguration elasticConfiguration)
    {
        try
        {
            var node = new Uri(elasticConfiguration.ElasticUrl);
            var settings = new ConnectionSettings(node)
                .DefaultIndex(indexName)
                .BasicAuthentication(elasticConfiguration.UserName, elasticConfiguration.Password)
                .DisableDirectStreaming();
            IElasticClient client = new ElasticClient(settings);
            return client;
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public static ElasticClient GetElasticConnection()
    {
        try
        {
            var node = new Uri("");
            var settings = new ConnectionSettings(node)
                .DefaultIndex(indexName)
                .BasicAuthentication("", "")
                .DisableDirectStreaming();
            return new ElasticClient(settings);
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<List<T>> AutoComplete(string field, string query, bool transport = false)
    {
        try
        {
            if (transport)
            {
                var responseT = await _elasticClient.SearchAsync<T>(s => s
                        .Index(indexName)
                        .Query(q => q
                        .Fuzzy(fz => fz.Field(field.ToLower())
                        .Value(query.ToLower()).Transpositions(true))
                ));
                if (responseT.Documents is null)
                    return null;
                return responseT.Documents.ToList();
            }
            var response = await _elasticClient.SearchAsync<T>(s => s
                   .Index(indexName)
                   .Query(q => q
                   .Fuzzy(fz => fz.Field(field.ToLower())
                   .Value(query.ToLower()).Fuzziness(Fuzziness.EditDistance(4))
                )
            ));
            if (response.Documents is null)
                return null;
            return response.Documents.ToList();
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<List<T>> AutoCompleteInBetween(string field, string query)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                    .From(0)
                    .Take(10)
                    .Index(indexName)
                    .Query(q => q
                    .Bool(b => b
                    .Should(m => m
                    .Wildcard(w => w
                    .Field(field.ToLower())
                    .Value(query.ToLower() + "*"))))));
            if (response.Documents is null)
                return null;
            return response.Documents.ToList();
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<List<T>> AutoMatchInBetween(string field, string query)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                .Index(indexName)
                .Query(q => q.MatchPhrasePrefix(m => m.Field(field.ToLower()).Query(query.ToLower()).MaxExpansions(10)))
            );
            if (response.Documents is null)
                return null;
            return response.Documents.ToList();
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<List<T>> AutoMatchWithoutSensitive(string field, string query)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                            .Index(indexName)
                        .Query(q => q
                    .MultiMatch(mm => mm
                    .Fields(f => f.Field(field.ToLower()))
                    .Type(TextQueryType.PhrasePrefix)
                    .Query(query.ToLower())
                    .MaxExpansions(10)
                )));
            if (response.Documents is null)
                return null;
            return response.Documents.ToList();
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }


    public async Task<List<T>> AutoAnalyzeWithLike(string field, string query)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                                        .Index(indexName)
                                    .Query(q => q
                              .QueryString(qs => qs
                            .AnalyzeWildcard()
                        .Query("*" + query.ToLower() + "*")
                    .Fields(fs => fs.Fields(field.ToLower())
                ))));
            if (response.Documents is null)
                return null;
            return response.Documents.ToList();
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<bool> IsConnect(bool configure)
    {
        try
        {
            if (configure)
            {
                var pingConfResponse = _elasticConfClient.Ping();
                if (pingConfResponse.IsValid)
                    return true;
                return false;
            }
            var pingResponse = _elasticClient.Ping();
            if (pingResponse.IsValid)
                return true;
            return false;
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error("Elasticsearch Error : " + ex.Message);
            return false;
        }
    }
}