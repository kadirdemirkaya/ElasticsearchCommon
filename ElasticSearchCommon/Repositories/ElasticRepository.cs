using ElasticSearchCommon.Models;
using Nest;

namespace ElasticSearchCommon.Repositories;

public class ElasticRepository<T> : BaseElasticSearchRepository<T>, IElasticSearchRepository<T> where T : BaseModel
{
    public override string IndexName { get; }

    public ElasticRepository(IElasticClient elasticClient) : base(elasticClient) => IndexName = typeof(T).Name.ToLower();
}
