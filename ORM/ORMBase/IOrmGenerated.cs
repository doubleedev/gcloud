using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMBase
{
    public interface IOrmGenerated : IDisposable
    {
        string GetTableName();
        void Map(IDataReader dataReader);
        string SqlSelect(bool withPkWhere);
        string SqlSelectPage(int pageSize, int page, bool withPkWhere);
        string SqlInsert();
        string SqlUpdate();
        string SqlDelete();
        List<SqlParameter> BaseWhereParameters();
        List<SqlParameter> InsertParameters();
        List<SqlParameter> UpdateParameters();
        string GetConnectionString();
        void InsertRow(DataTable dt);
        DataTable GenerateDataTable(OrmTableOwner owner = OrmTableOwner.dbo);

    }
}
