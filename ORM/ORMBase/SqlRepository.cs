using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StoresHub.DataAccess.ORMBase.Translator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ORMBase
{
    public class SqlRepository : BaseRepository
    {
        IConfigurationRoot _Configuration;
        StringBuilder _ClauseBuilder;

        public SqlRepository(IConfigurationRoot configuration, ILogger<SqlRepository> logger) : base(logger)
        {
            _Configuration = configuration;
        }

        public override List<T> ExecuteSql<T>(string sqlStatements, List<SqlParameter> parameters)
        {
            var baseObject = Activator.CreateInstance<T>();
            string sqlStatement = string.Empty;
            if (sqlStatements.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase) || sqlStatements.StartsWith("WITH", StringComparison.InvariantCultureIgnoreCase) || sqlStatements.StartsWith("EXEC", StringComparison.InvariantCultureIgnoreCase))
            {
                sqlStatement = sqlStatements;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(baseObject.SqlSelect(false));
                sb.Append(sqlStatements);
                sqlStatement = sb.ToString();
            }
            var returnList = new List<T>();
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                int tryCount = 0;
                while (tryCount < 3 && conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (InvalidOperationException)
                    {
                        tryCount++;
                        Thread.Sleep(500);
                        if (tryCount == 3)
                            throw;
                    }
                    tryCount = 100;
                }

                using (var cmd = new SqlCommand(sqlStatement, conn))
                {
                    if (parameters != null)
                        foreach (var p in parameters)
                        {
                            // handle null parameter values - AO
                            if (p.Value == null)
                                p.Value = DBNull.Value;

                            cmd.Parameters.Add(p);
                        }

                    cmd.CommandTimeout = 660;
                    using (var reader = cmd.ExecuteReader())
                        returnList = reader.MapDataToBusinessEntityCollection<T>();
                    
                }
                conn.Close();
            }

            return returnList;
        }

        public override void ExecuteProcedure(string sqlStatements, List<SqlParameter> parameters, string connectionName = "DefaultConnection")
        {
            var sb = new StringBuilder();
            sb.Append(sqlStatements);
            string sqlStatement = sb.ToString();

            using (var conn = new SqlConnection(_Configuration.GetConnectionString(connectionName)))
            {
                int tryCount = 0;
                while (tryCount < 3 && conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (InvalidOperationException)
                    {
                        tryCount++;
                        Thread.Sleep(500);
                        if (tryCount == 3)
                            throw;
                    }
                    tryCount = 100;
                }
                using (var cmd = new SqlCommand(sqlStatement, conn))
                {
                    if (parameters != null)
                        foreach (var p in parameters)
                        {
                            // handle null parameter values - AO
                            if (p.Value == null)
                                p.Value = DBNull.Value;

                            cmd.Parameters.Add(p);
                        }

                    cmd.CommandTimeout = 3600;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public override T ExecuteSqlSingle<T>(string sqlStatements, List<SqlParameter> parameters)
        {
            var baseObject = Activator.CreateInstance<T>();
            var sqlStatment = baseObject.SqlSelect(true);
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                int tryCount = 0;
                while (tryCount < 3 && conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (InvalidOperationException)
                    {
                        tryCount++;
                        Thread.Sleep(500);
                        if (tryCount == 3)
                            throw;
                    }
                    tryCount = 100;
                }
                using (var cmd = new SqlCommand(sqlStatment, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var obj = Activator.CreateInstance<T>();
                                obj.Map(reader);
                                return obj;

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log($"SQL Get All Error: {e.Message}", LogLevel.Error, "Sql",e);
                        throw;
                    }

                }
                conn.Close();
            }

            return default(T);
        }

        public override List<T> GetAll<T>()
        {
            var baseObject = Activator.CreateInstance<T>();
            var sqlStatment = baseObject.SqlSelect(false);
            var returnList = new List<T>();
            Log($"Executing Sql {sqlStatment}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                int tryCount = 0;
                while (tryCount < 3 && conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (InvalidOperationException)
                    {
                        tryCount++;
                        Thread.Sleep(500);
                        if (tryCount == 3)
                            throw;
                    }
                    tryCount = 100;
                }
                using (var cmd = new SqlCommand(sqlStatment, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            returnList = reader.MapDataToBusinessEntityCollection<T>();
                        }
                conn.Close();
            }

            return returnList;
        }

        public override List<T> GetAll<T>(int pageSize, int page)
        {
            throw new NotImplementedException();
        }

        public override T GetSingle<T>(Expression<Func<T, bool>> where)
        {
            if (where == null)
                throw new ArgumentNullException("Where clause cannot be null");

            var sb = new StringBuilder();
            var baseObject = Activator.CreateInstance<T>();
            sb.AppendLine(baseObject.SqlSelect(false));

           // var exp = ((LambdaExpression)where).Body;
            _ClauseBuilder = new StringBuilder();
            Process(where);

            var processedWhere = _ClauseBuilder.ToString();

            sb.AppendLine("WHERE ");
            sb.AppendLine(processedWhere);
            Log(sb.ToString(), LogLevel.Debug, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sb.ToString(), conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                baseObject.Map(reader);
                                break;
                            }
                        }
                }
                catch (Exception e)
                {
                    Log($"SQL Get Single Error: {e.Message}", LogLevel.Information, "Sql",e);
                }
                conn.Close();
            }

            return baseObject;
        }

        void Process<T>(Expression<Func<T, bool>> e)
        {
            QueryTranslator queryTranslator = new QueryTranslator();

            _ClauseBuilder.Append(queryTranslator.Translate<T>(e));
        }

        public override List<T> GetWhere<T>(Dictionary<string, object> parameters)
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

            return ExecuteSql<T>(sb.ToString(), sqlParams);
        }

        public override List<T> GetWhere<T>(Expression<Func<T, bool>> where)
        {
            if (where == null)
                throw new ArgumentNullException("Where clause cannot be null");

            var sb = new StringBuilder();
            var baseObject = Activator.CreateInstance<T>();
            sb.AppendLine(baseObject.SqlSelect(false));
            _ClauseBuilder = new StringBuilder();
            Process(where);

            var processedWhere = _ClauseBuilder.ToString();

            sb.AppendLine("WHERE ");
            sb.AppendLine(processedWhere);

            Log($"Executing Sql {sb.ToString()}", LogLevel.Information, "Sql");

            var returnList = new List<T>();
            Log(sb.ToString(),LogLevel.Debug,"Sql");

            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sb.ToString(), conn))
                    using (var reader = cmd.ExecuteReader())
                        returnList = reader.MapDataToBusinessEntityCollection<T>();
                }
                catch (Exception e)
                {
                    Log($"SQL Get Where Error: {e.Message}", LogLevel.Information, "Sql",e);                    
                }
                conn.Close();
            }

            return returnList;
        }

        public override long Count<T>()
        {
            throw new NotImplementedException();
        }

        public override long Count<T>(Expression<Func<T, bool>> where)
        {
            if (where == null)
                throw new ArgumentNullException("Where clause cannot be null");

            var sb = new StringBuilder();
            var baseObject = Activator.CreateInstance<T>();
            sb.AppendLine("SELECT COUNT(1) FROM " + baseObject.GetTableName() + " as runSql ");

            _ClauseBuilder = new StringBuilder();
            Process(where);
            var processedWhere = _ClauseBuilder.ToString();

            sb.AppendLine("WHERE ");
            sb.AppendLine(processedWhere);

            long result = 0;
            Log($"Executing Sql {sb.ToString()}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sb.ToString(), conn))
                        result = long.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception e)
                {
                    Log($"SQL Count Error: {e.Message}", LogLevel.Error, "Sql",e);
                }
                conn.Close();
            }

            return result;
        }

        public override async Task BulkInsert<T>(DataTable dataTable)
        {
            var baseObject = Activator.CreateInstance<T>();
            Log($"Executing Bulk Update for {typeof(T).FullName}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                await conn.OpenAsync();

                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
                    {
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        bulkCopy.ColumnMappings.Clear();

                        foreach (DataColumn col in dataTable.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }

                        await bulkCopy.WriteToServerAsync(dataTable);
                        bulkCopy.Close();
                    }
                }
                catch (SqlException e)
                {
                    Log($"{e.Message} {e.StackTrace}", LogLevel.Error, "sql",e);
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public override object Insert<T>(T model)
        {
            var baseObject = Activator.CreateInstance<T>();
            var sqlStatement = model.SqlInsert();
            var parameters = model.InsertParameters();
            object ret = null;
            Log($"Executing Sql {sqlStatement}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }

                    using (var cmd = new SqlCommand(sqlStatement, conn))
                    {
                        foreach (var p in parameters)
                        {
                            // handle null parameter values - AO
                            if (p.Value == null)
                                p.Value = DBNull.Value;

                            cmd.Parameters.Add(p);
                        }

                        try
                        {
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "SELECT @@IDENTITY";
                            ret = cmd.ExecuteScalar();
                        }
                        catch (SqlException sqlEx)
                        {
                            throw sqlEx;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log($"SQL Insert Error: {e.Message}", LogLevel.Information, "Sql",e);
                }
                conn.Close();
            }

            return ret;
        }

        public override void Update<T>(T model)
        {
            var baseObject = Activator.CreateInstance<T>();
            var sqlStatement = model.SqlUpdate();
            var parameters = model.UpdateParameters();
            Log($"Executing Sql {sqlStatement}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sqlStatement, conn))
                    {
                        foreach (var p in parameters)
                        {
                            // handle null parameter values - AO
                            if (p.Value == null)
                                p.Value = DBNull.Value;

                            cmd.Parameters.Add(p);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Log($"SQL Update Error: {e.Message}", LogLevel.Error, "Sql",e);
                }
                conn.Close();
            }
        }

        public override void Delete<T>(T model)
        {

            var baseObject = Activator.CreateInstance<T>();
            var sqlStatement = model.SqlDelete();
            var parameters = model.BaseWhereParameters();

            Log($"Executing Sql {sqlStatement}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sqlStatement, conn))
                    {
                        foreach (var p in parameters)
                        {
                            // handle null parameter values - AO
                            if (p.Value == null)
                                p.Value = DBNull.Value;

                            cmd.Parameters.Add(p);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Log($"SQL Delete Error: {e.Message}", LogLevel.Error, "Sql",e);
                }
                conn.Close();
            }
        }
        public override void Truncate<T>(string schema, string tableName)
        {
            var baseObject = Activator.CreateInstance<T>();
            var sqlStatement = $"TRUNCATE TABLE [{schema}].[{tableName}]";

            Log($"Executing Sql {sqlStatement}", LogLevel.Information, "Sql");
            using (var conn = new SqlConnection(_Configuration.GetConnectionString(baseObject.GetConnectionString())))
            {
                try
                {
                    int tryCount = 0;
                    while (tryCount < 3 && conn.State != ConnectionState.Open)
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (InvalidOperationException)
                        {
                            tryCount++;
                            Thread.Sleep(500);
                            if (tryCount == 3)
                                throw;
                        }
                        tryCount = 100;
                    }
                    using (var cmd = new SqlCommand(sqlStatement, conn))
                        cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log($"SQL Truncate Error: {e.Message}", LogLevel.Information, "Sql",e);
                }

                conn.Close();
            }
        }
    }
}
