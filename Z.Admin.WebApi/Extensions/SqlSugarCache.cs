using SqlSugar;
using Z.Common.Cache;

namespace Z.Admin.WebApi.Extensions
{
    public class SqlSugarCache : ICacheService
    {
        public void Add<V>(string key, V value)
        {
            CacheHelper.SetCache(key, value);
        }

        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
            CacheHelper.SetCache(key, value, cacheDurationInSeconds);
        }

        public bool ContainsKey<V>(string key)
        {
            return CacheHelper.Exists(key);
        }

        public V Get<V>(string key)
        {
            return (V)CacheHelper.Get(key);
        }

        public IEnumerable<string> GetAllKey<V>()
        {
            return CacheHelper.GetCacheKeys();
        }

        public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
        {
            if (ContainsKey<V>(cacheKey))
            {
                var result = Get<V>(cacheKey);
                if (result == null)
                {
                    return create();
                }
                else
                {
                    return result;
                }
            }
            else
            {
                var result = create();

                Add(cacheKey, result, cacheDurationInSeconds);
                return result;
            }
        }

        public void Remove<V>(string key)
        {
            CacheHelper.Remove(key);
        }
    }
}
