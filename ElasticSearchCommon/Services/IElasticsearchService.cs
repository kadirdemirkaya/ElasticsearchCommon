using ElasticSearchCommon.Settings;
using Nest;

namespace ElasticSearchCommon.Services;

public interface IElasticsearchService<T> where T : class
{
    public Task<bool> IsConnect(bool configure);

    public Task<ElasticClient> GetConnection(ElasticConfiguration elasticConfiguration);

    public Task<List<T>> AutoComplete(string field, string query, bool transport = false);

    public Task<List<T>> AutoCompleteInBetween(string field, string query);

    public Task<List<T>> AutoMatchInBetween(string field, string query);

    public Task<List<T>> AutoMatchWithoutSensitive(string field, string query);

    public Task<List<T>> AutoAnalyzeWithLike(string field, string query);

}