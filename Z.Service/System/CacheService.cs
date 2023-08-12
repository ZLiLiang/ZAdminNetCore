using System.Collections.Generic;
using Z.Common.Cache;

namespace Z.Service.System
{
    public class CacheService
    {
        #region 用户权限 缓存
        public static List<string> GetUserPerms(string key)
        {
            return (List<string>)CacheHelper.GetCache(key);
            //return RedisServer.Cache.Get<List<string>>(key).ToList();
        }

        public static void SetUserPerms(string key, object data)
        {
            CacheHelper.SetCache(key, data);
            //RedisServer.Cache.Set(key, data);
        }
        public static void RemoveUserPerms(string key)
        {
            CacheHelper.Remove(key);
            //RedisServer.Cache.Del(key);
        }
        #endregion
    }
}
