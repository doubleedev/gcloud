using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMBase
{
    public interface IRepository : IDisposable
    {
        List<T> GetAll<T>() where T : IOrmGenerated;
        T GetSingle<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;
        List<T> GetAll<T>(int pageSize, int page) where T : IOrmGenerated;
        List<T> GetWhere<T>(Dictionary<string, object> parameters) where T : IOrmGenerated;
        List<T> GetWhere<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;
        List<T> ExecuteSql<T>(string sqlStatements, List<SqlParameter> parameters) where T : IOrmGenerated;
        void ExecuteProcedure(string sqlStatements, List<SqlParameter> parameters, string connectionName = "DefaultConnection");
        T ExecuteSqlSingle<T>(string sqlStatements, List<SqlParameter> parameters) where T : IOrmGenerated;
        object Insert<T>(T model) where T : IOrmGenerated;
        void Update<T>(T model) where T : IOrmGenerated;
        void Delete<T>(T model) where T : IOrmGenerated;
        long Count<T>() where T : IOrmGenerated;
        long Count<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;
    }
}
