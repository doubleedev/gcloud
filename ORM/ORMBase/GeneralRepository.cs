using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ORMBase;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ARGORM.Repository.ORMBase
{
    public class GeneralRepository : IGeneralRepository
    {
        readonly IConfigurationRoot _Configuration;
        readonly ILogger<SqlRepository> _Logger;
        public GeneralRepository(IConfigurationRoot configuration, ILogger<SqlRepository> logger)
        {
            _Configuration = configuration;
            _Logger = logger;
        }
        public IEnumerable<T> GetAll<T>() where T : IOrmGenerated
        {
            using (var rep = new SqlRepository(_Configuration, _Logger))
                return rep.GetAll<T>();
        }

        public T GetSingle<T>(string field, object value) where T : IOrmGenerated
        {
            using (var rep = new SqlRepository(_Configuration, _Logger))
                return rep.ExecuteSqlSingle<T>($"WHERE [{field}] = @{field}", new List<System.Data.SqlClient.SqlParameter>{
                    new System.Data.SqlClient.SqlParameter($"@{field}",value) });
        }

        public IEnumerable<T> GetWhere<T>(string field, object value) where T : IOrmGenerated
        {
            using (var rep = new SqlRepository(_Configuration, _Logger))
                return rep.ExecuteSql<T>($"WHERE [{field}] = @{field}", new List<System.Data.SqlClient.SqlParameter>{
                    new System.Data.SqlClient.SqlParameter($"@{field}",value) });
        }

        public IEnumerable<T> GetWhere<T>(Dictionary<string, object> parameters) where T : IOrmGenerated
        {
            var sb = new StringBuilder();
            sb.AppendLine("WHERE ");
            var sqlParams = new List<SqlParameter>();
            bool first = true;
            foreach (var o in parameters)
            {
                sb.AppendLine($"{(first ? "" : " AND ")} [{o.Key}] = @{o.Key.Replace(" ", "")} ");
                sqlParams.Add(new SqlParameter($"@{o.Key.Replace(" ", "")}", o.Value));
                first = false;
            }

            using (var rep = new SqlRepository(_Configuration, _Logger))
                return rep.ExecuteSql<T>(sb.ToString(), sqlParams);
        }

        public object Insert<T>(T model) where T : IOrmGenerated
        {
            using (var rep = new SqlRepository(_Configuration, _Logger))
                return rep.Insert(model);
        }

        public void Update<T>(T model) where T : IOrmGenerated
        {
            using (var rep = new SqlRepository(_Configuration, _Logger))
                rep.Update(model);
        }
    }
}
