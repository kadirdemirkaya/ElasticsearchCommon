using ElasticSearchCommon.Models;
using Nest;
using Serilog;

namespace ElasticSearchCommon.Repositories;

public abstract class BaseElasticSearchRepository<T> : IBaseElasticSearchRepository<T> where T : BaseModel
{
    private readonly IElasticClient _elasticClient;
    public abstract string IndexName { get; }

    protected BaseElasticSearchRepository(IElasticClient elasticClient) => _elasticClient = elasticClient;

    public async Task<bool> ChekIndexAsync(string indexName)
    {
        try
        {
            var anyy = await _elasticClient.Indices.ExistsAsync(indexName);
            if (anyy.Exists)
                return true;
            return false;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }
    }

    public async Task<T> GetAsync(string id)
    {
        try
        {
            var response = await _elasticClient.GetAsync(DocumentPath<T>.Id(id).Index(IndexName));
            if (response.IsValid)
                return response.Source;
            Log.Error(response.OriginalException, response.ServerError?.ToString()!);
            return null;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null;
        }
    }

    public async Task<T> GetAsync(IGetRequest request)
    {
        try
        {
            var response = await _elasticClient.GetAsync<T>(request);
            if (response.IsValid)
                return response.Source;
            Log.Error(response.OriginalException, response.ServerError?.ToString()!);
            return null;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null;
        }

    }

    public async Task<T> FindAsync(string id)
    {
        try
        {
            var response = await _elasticClient.GetAsync(DocumentPath<T>.Id(id).Index(IndexName));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response.Source;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null;
        }

    }

    public async Task<T> FindAsync(IGetRequest request)
    {
        try
        {
            var response = await _elasticClient.GetAsync<T>(request);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response.Source;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null;
        }

    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var search = new SearchDescriptor<T>(IndexName).MatchAll();
            var response = await _elasticClient.SearchAsync<T>(search);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response.Hits.Select(hit => hit.Source).ToList();
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null!;
        }
    }

    public async Task<IEnumerable<T>> GetManyAsync(IEnumerable<string> ids)
    {
        try
        {
            var response = await _elasticClient.GetManyAsync<T>(ids, IndexName);
            return response.Select(item => item.Source).ToList();
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null!;
        }

    }

    public async Task<IEnumerable<T>> SearchAsync(Func<QueryContainerDescriptor<T>, QueryContainer> request)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s =>
                                    s.Index(IndexName)
                                    .Query(request));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response.Hits.Select(hit => hit.Source).ToList();
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null!;
        }

    }

    public async Task<ISearchResponse<T>> SearchAsync(Func<QueryContainerDescriptor<T>, QueryContainer> request,
        Func<AggregationContainerDescriptor<T>, IAggregationContainer> aggregationsSelector)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(s =>
         s.Index(IndexName)
             .Query(request)
             .Aggregations(aggregationsSelector));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null!;
        }

    }

    public async Task<IEnumerable<T>> SearchAsync(Func<SearchDescriptor<T>, ISearchRequest> selector)
    {
        try
        {
            var list = new List<T>();
            var response = await _elasticClient.SearchAsync(selector);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return null;
            }

            return response.Hits.Select(hit => hit.Source).ToList();
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return null!;
        }

    }

    public async Task<bool> CreateIndexAsync()
    {
        try
        {
            if (!(await _elasticClient.Indices.ExistsAsync(IndexName)).Exists)
            {
                await _elasticClient.Indices.CreateAsync(IndexName, c =>
                {
                    c.Map<T>(p => p.AutoMap());
                    return c;
                });
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }
    }

    public async Task<bool> CreateIndexAsync(string indexName)
    {
        try
        {
            if (!(await _elasticClient.Indices.ExistsAsync(indexName)).Exists)
            {
                await _elasticClient.Indices.CreateAsync(indexName, c =>
                {
                    c.Map<T>(p => p.AutoMap());
                    return c;
                });
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        try
        {
            var response = await _elasticClient.Indices.DeleteAsync(indexName);
            if (response.IsValid)
                return true;
            Log.Error(response.OriginalException, response.ServerError?.ToString()!);
            return false;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }
    }

    public async Task<bool> CreateIndexWithMapAsync(string indexName, Func<TypeMappingDescriptor<object>, ITypeMapping> mappingDescriptor)
    {
        try
        {
            var anyy = await _elasticClient.Indices.ExistsAsync(indexName);
            if (anyy.Exists)
                return false;

            var response = await _elasticClient.Indices.CreateAsync(indexName,
                ci => ci
                    .Index(indexName)
                    .Map(mappingDescriptor)
                    .Settings(s => s.NumberOfShards(3).NumberOfReplicas(1))
                    );
            if (response.IsValid)
                return true;
            Log.Error(response.OriginalException, response.ServerError?.ToString()!);
            return false;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> InsertAsync(T model)
    {
        try
        {
            var response = await _elasticClient.IndexAsync(model, descriptor => descriptor.Index(IndexName));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> InsertManyAsync(IList<T> models)
    {
        try
        {
            await CreateIndexAsync();
            var response = await _elasticClient.IndexManyAsync(models, IndexName);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> UpdateAsync(T model)
    {
        try
        {
            var response = await _elasticClient.UpdateAsync(DocumentPath<T>.Id(model.Id).Index(IndexName), p => p.Doc(model));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> UpdatePartAsync(T model, object partialEntity)
    {
        try
        {
            var request = new UpdateRequest<T, object>(IndexName, model.Id)
            {
                Doc = partialEntity
            };
            var response = await _elasticClient.UpdateAsync(request);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        try
        {
            var response = await _elasticClient.DeleteAsync(DocumentPath<T>.Id(id).Index(IndexName));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<bool> DeleteByQueryAsync(Func<QueryContainerDescriptor<T>, QueryContainer> selector)
    {
        try
        {
            var response = await _elasticClient.DeleteByQueryAsync<T>(q => q
           .Query(selector)
           .Index(IndexName)
            );

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }

    }

    public async Task<long> GetTotalCountAsync()
    {
        try
        {
            var search = new SearchDescriptor<T>(IndexName).MatchAll();
            var response = await _elasticClient.SearchAsync<T>(search);

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return default;
            }

            return response.Total;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return default;
        }

    }

    public async Task<bool> ExistAsync(string id)
    {
        try
        {
            var response = await _elasticClient.DocumentExistsAsync(DocumentPath<T>.Id(id).Index(IndexName));

            if (!response.IsValid)
            {
                Log.Error(response.OriginalException, response.ServerError?.ToString()!);
                return false;
            }

            return response.Exists;
        }
        catch (System.Exception ex)
        {
            Log.Error("ElasticSearch Error : " + ex.Message);
            return false;
        }
    }
}