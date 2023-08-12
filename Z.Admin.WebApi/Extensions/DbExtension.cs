using Infrastructure;
using Infrastructure.App;
using Infrastructure.Extensions;
using NLog;
using SqlSugar;
using SqlSugar.IOC;
using System.Data;
using Z.Model;
using Z.Model.System;

namespace Z.Admin.WebApi.Extensions
{
    /// <summary>
    /// sqlsugar 数据处理
    /// </summary>
    public static class DbExtension
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化db
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        /// <param name="environment"></param>
        public static void AddDb(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            List<DbConfigs> dbConfigs = configuration.GetSection("DbConfigs").Get<List<DbConfigs>>();

            var iocList = new List<IocConfig>();
            foreach (var item in dbConfigs)
            {
                iocList.Add(new IocConfig()
                {
                    ConfigId = item.ConfigId,
                    ConnectionString = item.Conn,
                    DbType = (IocDbType)item.DbType,
                    IsAutoCloseConnection = item.IsAutoCloseConnection,
                });
            }
            SugarIocServices.AddSqlSugar(iocList);
            ICacheService cache = new SqlSugarCache();
            SugarIocServices.ConfigurationSugar(action =>
            {
                var u = App.User;
                if (u != null)
                {
                    DataPermi.FilterData(0);
                }

                iocList.ForEach(iocConfig =>
                {
                    SetSugarApp(action, iocConfig, cache);
                });
            });

            if (configuration["InitDb"].ParseToBool() == true && environment.IsDevelopment())
            {
                InitTable.InitDb();
            }
        }

        /// <summary>
        /// 数据库Aop设置
        /// </summary>
        /// <param name="db"></param>
        /// <param name="iocConfig"></param>
        /// <param name="cache"></param>
        private static void SetSugarApp(SqlSugarClient db,IocConfig iocConfig,ICacheService cache)
        {
            var config = db.GetConnectionScope(iocConfig.ConfigId).CurrentConnectionConfig;

            string configId = config.ConfigId;
            db.GetConnectionScope(configId).Aop.OnLogExecuted = (sql, pars) =>
            {
                string log = $"【db{configId}SQL语句】{UtilMethods.GetSqlString(config.DbType, sql, pars)}\n";
                if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info(log);
                }
                else if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn(log);
                }
                else if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("TRUNCATE", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Error(log);
                }
                else
                {
                    log = $"【db{configId}SQL语句】dbo.{sql} {string.Join(",", pars.Select(selector => selector.ParameterName + "=" + GetParsValue(selector)))};\n";
                }
            };
            db.GetConnectionScope(configId).Aop.OnError = (ex) =>
            {
                string sql = "【错误SQL】" + UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n";
                logger.Error(ex, $"{sql}\r\n{ex.Message}\r\n{ex.StackTrace}");
            };
            db.GetConnectionScope(configId).Aop.DataExecuting = (oldValue, entiyInfo) =>
            {

            };
            //差异日志功能
            db.GetConnectionScope(configId).Aop.OnDiffLogEvent = it =>
            {
                //操作前记录  包含： 字段描述 列名 值 表名 表描述
                var editBeforeData = it.BeforeData;//插入Before为null，之前还没进库
                //操作后记录   包含： 字段描述 列名 值  表名 表描述
                var editAfterData = it.AfterData;
                var sql = it.Sql;
                var parameter = it.Parameters;
                var data = it.BusinessData;//这边会显示你传进来的对象
                var time = it.Time;
                var diffType = it.DiffType;

                if (diffType == DiffType.delete)
                {
                    string name = App.UserName;

                    foreach (var item in editBeforeData)
                    {
                        var pars = db.Utilities.SerializeObject(item.Columns.ToDictionary(it => it.ColumnName, it => it.Value));

                        SqlDiffLog log = new()
                        {
                            BeforeData = pars,
                            BusinessData = data.ToString(),
                            DiffType = diffType.ToString(),
                            Sql = sql,
                            TableName = item.TableName,
                            UserName = name,
                            AddTime = DateTime.Now,
                            ConfigId = configId,
                        };
                        db.GetConnectionScope(0).Insertable(log).ExecuteReturnSnowflakeId();
                    }
                }
            };
            db.GetConnectionScope(configId).CurrentConnectionConfig.MoreSettings = new ConnMoreSettings()
            {
                IsAutoRemoveDataCache = true,
            };
            db.GetConnectionScope(configId).CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = cache,
                EntityService = (c, p) =>
                {
                    if (p.IsPrimarykey == true)//主键不能为null
                    {
                        p.IsNullable = false;
                    }
                    else if (p.ExtendedAttribute?.ToString() == ProteryConstant.NOTNULL.ToString())
                    {
                        p.IsNullable = false;
                    }
                    else//则否默认为null
                    {
                        p.IsNullable = true;
                    }

                    if (config.DbType == SqlSugar.DbType.PostgreSQL)
                    {
                        if (c.Name == nameof(SysMenu.IsCache) || c.Name == nameof(SysMenu.IsFrame))
                        {
                            p.DataType = "char(1)";
                        }
                    }

                    #region 兼容Oracle

                    if (config.DbType == SqlSugar.DbType.Oracle)
                    {
                        if (p.IsIdentity == true)
                        {
                            if (p.EntityName == nameof(SysUser))
                            {
                                p.OracleSequenceName = "SEQ_SYS_USER_USERID";
                            }
                            else if (p.EntityName == nameof(SysRole))
                            {
                                p.OracleSequenceName = "SEQ_SYS_ROLE_ROLEID";
                            }
                            else if (p.EntityName == nameof(SysDept))
                            {
                                p.OracleSequenceName = "SEQ_SYS_DEPT_DEPTID";
                            }
                            else if (p.EntityName == nameof(SysMenu))
                            {
                                p.OracleSequenceName = "SEQ_SYS_MENU_MENUID";
                            }
                            else
                            {
                                p.OracleSequenceName = "SEQ_ID";
                            }
                        }
                    }

                    #endregion
                }
            };
        }

        private static object GetParsValue(SugarParameter parameter)
        {
            if (parameter.DbType == System.Data.DbType.String || parameter.DbType == System.Data.DbType.DateTime || parameter.DbType == System.Data.DbType.String)
            {
                return "`" + parameter.Value + "`";
            }
            return parameter.Value;
        }
    }
}
