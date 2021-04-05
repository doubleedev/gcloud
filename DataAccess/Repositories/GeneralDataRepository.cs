using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ORMBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class GeneralDataRepository : IGeneralDataRepository
	{
		private readonly IConfigurationRoot _Configuration;
		private readonly ILogger<SqlRepository> _Logger;
		private readonly int _ParameterLimit = 6000;

		public GeneralDataRepository(IConfigurationRoot configuration, ILogger<SqlRepository> logger)
		{
			_Configuration = configuration;
			_Logger = logger;
		}

		public IEnumerable<T> GetAll<T>() where T : IOrmGenerated
		{
			using (var rep = new SqlRepository(_Configuration, _Logger))
				return rep.GetAll<T>() ?? new List<T>();
		}

		public T GetSingle<T>(string field, object value, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated
		{
			using (var rep = new SqlRepository(_Configuration, _Logger))
			{
				Sanitise(field);
				if (conditional == DataConditional.NotNull || conditional == DataConditional.IsNull)
				{
					var sql = $"WHERE [{field}] {conditional.GetDataConditional()}";
					var ret = rep.ExecuteSqlSingle<T>(sql, new List<SqlParameter>());
					return ret;
				}
				else
				{
					var sql = $"WHERE [{field}] {conditional.GetDataConditional()} @{field}";
					var ret = rep.ExecuteSqlSingle<T>(sql, new List<SqlParameter>{
					new SqlParameter($"@{field}",value) });
					return ret;
				}
			}
		}

		public IEnumerable<T> GetWhere<T>(string field, object value, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated
		{
			Sanitise(field);
			using (var rep = new SqlRepository(_Configuration, _Logger))
				if (conditional == DataConditional.NotNull || conditional == DataConditional.IsNull)
					return rep.ExecuteSql<T>($"WHERE [{field}] {conditional.GetDataConditional()}", new List<SqlParameter>()) ?? new List<T>();
				else
					return rep.ExecuteSql<T>($"WHERE [{field}] {conditional.GetDataConditional()} @{field}", new List<SqlParameter>{
					new SqlParameter($"@{field}",value) }) ?? new List<T>();
		}

		public IEnumerable<T> GetWhere<T>(Dictionary<string, object> parameters, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated
		{
			var sb = new StringBuilder();
			sb.AppendLine("WHERE ");
			var sqlParams = new List<SqlParameter>();
			bool first = true;
			int paramIndex = 0;
			Sanitise(parameters);
			foreach (var o in parameters)
			{
				var paramName = $"sqlParam_{paramIndex}";
				if (conditional == DataConditional.NotNull || conditional == DataConditional.IsNull)
					sb.AppendLine($"{(first ? "" : " AND ")} [{o.Key}] {conditional.GetDataConditional()} ");
				else
				{
					sb.AppendLine($"{(first ? "" : " AND ")} [{o.Key}] {conditional.GetDataConditional()} @{paramName.Replace(" ", "")} ");
					sqlParams.Add(new SqlParameter($"@{paramName.Replace(" ", "")}", o.Value));
				}
				first = false;
				paramIndex++;
			}

			using (var rep = new SqlRepository(_Configuration, _Logger))
				return rep.ExecuteSql<T>(sb.ToString(), sqlParams) ?? new List<T>();
		}

		public IEnumerable<T> GetWhere<T>(IEnumerable<GeneralDataParameter> parameters, IEnumerable<string> inParameters = null) where T : IOrmGenerated
		{
			Sanitise(parameters);
			var ret = new List<T>();

			var parms = new List<List<GeneralDataParameter>>();

			if (parameters.Any(p => p.IsGroup) && parameters.Count() > _ParameterLimit)
				throw new Exception($"Grouping cannot be because the total number of parameters is above the limit of {_ParameterLimit}");

			if (parameters.GroupBy(p => p.Field).Count() == 1)
				while (parameters.Any())
				{
					parms.Add(parameters.Take(_ParameterLimit).ToList());
					parameters = parameters.Skip(_ParameterLimit);
				}
			else
				parms.Add(parameters.ToList());

			var groups = new List<GeneralDataParameter>();

			foreach (var block in parms)
			{
				var sb = new StringBuilder();
				sb.AppendLine("WHERE ");
				var sqlParams = new List<SqlParameter>();
				bool first = true;
				int paramIndex = 0;
				foreach (var o in block)
				{
					if (o.IsGroup)
					{
						if (!o.OpenGroup)
						{
							if (!groups.Any())
								throw new Exception("End of group without start.");
							groups.Remove(groups.Last());
							sb.AppendLine(")");
							first = false;
						}
						else
						{
							groups.Add(o);
							sb.AppendLine($"{(first ? "(" : $" {o.MultiConditionType} ")} ( ");
							first = true;
						}
					}
					else
					{
						var paramName = $"sqlParam_{paramIndex}";
						if (o.Conditional == DataConditional.NotNull || o.Conditional == DataConditional.IsNull)
							sb.AppendLine($"{(first ? "" : $" {o.MultiConditionType} ")} [{o.Field}] {o.Conditional.GetDataConditional()} ");
						else if (o.Conditional == DataConditional.In)
						{
							if (inParameters == null || !inParameters.Any())
								throw new NullReferenceException("No inParameters passed with 'IN' conditional");

							sb.AppendLine($"{(first ? "" : $" {o.MultiConditionType} ")} [{o.Field}] {o.Conditional.GetDataConditional()} ({string.Join(',', inParameters)}) ");
						}
						else
						{
							sb.AppendLine($"{(first ? "" : $" {o.MultiConditionType} ")} [{o.Field}] {o.Conditional.GetDataConditional()} @{paramName.Replace(" ", "")} ");
							sqlParams.Add(new SqlParameter($"@{paramName.Replace(" ", "")}", o.Value));
						}
						first = false;
					}
					paramIndex++;
				}

				//close off any groups left open
				if (groups.Any())
				{
					foreach (var g in groups)
						sb.AppendLine(")");
				}

				using (var rep = new SqlRepository(_Configuration, _Logger))
				{
					var data = rep.ExecuteSql<T>(sb.ToString(), sqlParams);
					if (data.Any())
						ret.AddRange(data);
				}
			}

			return ret.Any() ? ret : new List<T>();

		}

		public Task<IEnumerable<D>> GetDetailForHeaders<H, D>(IEnumerable<H> headers, string headerIdColumn, string detailIdColumn) where H : IOrmGenerated where D : IOrmGenerated
		{
			return Task.Run(() =>
			{
				Sanitise(headerIdColumn);
				Sanitise(detailIdColumn);
				var sb = new StringBuilder();
				var sqlParams = new List<SqlParameter>();
				sqlParams.Add(new SqlParameter("idValues", SqlDbType.NVarChar) { Value = $"{string.Join(',', headers.Select(h => h.GetType().GetProperty(headerIdColumn).GetValue(h)))}" });

				sb.AppendLine($"WHERE  {detailIdColumn} IN (SELECT * FROM dbo.CSVToTable(@idValues))");

				using (var rep = new SqlRepository(_Configuration, _Logger))
				{
					var data = rep.ExecuteSql<D>(sb.ToString(), sqlParams);
					if (data.Any())
					{
						var ret = new List<D>();
						foreach (var rec in data.GroupBy(d => d.GetType().GetProperty(detailIdColumn).GetValue(d)))
						{
							ret.AddRange(rec.ToList());
						}
						return (IEnumerable<D>)ret;
					}
					else
					{
						return new List<D>();
					}
				}
			});
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

		public void Delete<T>(T model) where T : IOrmGenerated
		{
			using (var rep = new SqlRepository(_Configuration, _Logger))
				rep.Delete(model);
		}

		public void BulkDelete<T>(IEnumerable<T> model, IEnumerable<string> fields, OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated
		{
			var sb = new StringBuilder();
			bool firstRec = true;
			bool firstField = true;
			sb.Append($"DELETE FROM {owner}.{((IOrmGenerated)Activator.CreateInstance(typeof(T))).GetTableName()}");
			Sanitise(fields);

			foreach (var r in model)
			{
				if (firstRec)
					sb.Append(" WHERE (");
				else
					sb.Append(" OR (");
				foreach (var field in fields)
				{
					var dec = "";
					var objField = typeof(T).GetProperty(field);
					var val = (objField).GetValue(r);
					if (val is string || val is DateTime)
						dec = "'";
					if (firstField)
						sb.Append($"{field} = {dec}{val}{dec}");
					else
						sb.Append($" AND {field} = {dec}{val}{dec}");
					firstField = false;
				}
				sb.Append(")");
				firstRec = false;
				firstField = true;
			}

			using (var rep = new SqlRepository(_Configuration, _Logger))
				rep.ExecuteSql<T>(sb.ToString(), null);
		}

		public long Count<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated
		{
			using (var rep = new SqlRepository(_Configuration, _Logger))
				return rep.Count(where);
		}

		public IEnumerable<T> ExecuteSql<T>(string sql, List<SqlParameter> parameters) where T : IOrmGenerated
		{
			Sanitise(parameters);
			using (var rep = new SqlRepository(_Configuration, _Logger))
				return rep.ExecuteSql<T>(sql, parameters) ?? new List<T>();
		}

		public void Truncate<T>(OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated
		{
			using (var rep = new SqlRepository(_Configuration, _Logger))
				rep.Truncate<T>(owner.ToString(), ((IOrmGenerated)Activator.CreateInstance(typeof(T))).GetTableName()); //Enum.GetName(typeof(OrmTableOwner), owner)
		}

		public async Task<bool> BulkInsert<T>(DataTable dt) where T : IOrmGenerated
		{
			try
			{
				Sanitise(dt);
				using (var rep = new SqlRepository(_Configuration, _Logger))
					await rep.BulkInsert<T>(dt);
			}
			catch (Exception e)
			{
				_Logger.LogError(e, $"{e.Message}");
				throw;
			}

			return true;
		}

		//Sanitise SQL based on data table
		public DataTable Sanitise(DataTable dt)
		{
			foreach (DataColumn c in dt.Columns)
				Sanitise(c.ColumnName);

			return dt;
		}

		//Sanitise SQL based on generalDataParameters
		public IEnumerable<GeneralDataParameter> Sanitise(IEnumerable<GeneralDataParameter> parameters)
		{
			foreach (var p in parameters)
				Sanitise(p.Field);

			return parameters;
		}

		//Sanitise SQL based on SqlParameters
		public IEnumerable<SqlParameter> Sanitise(IEnumerable<SqlParameter> parameters)
		{
			foreach (var p in parameters)
				Sanitise(p.ParameterName.StartsWith('@') ? p.ParameterName.Substring(1) : p.ParameterName);

			return parameters;
		}

		//Sanitise SQL based on dictionay fields
		public Dictionary<string, object> Sanitise(Dictionary<string, object> fields)
		{
			return fields.ToDictionary(fk => Sanitise(fk.Key), fv => Sanitise(fv));
		}

		//Sanitise SQL based field names
		public IEnumerable<string> Sanitise(IEnumerable<string> fields)
		{
			return fields.Select(f => Sanitise(f));
		}

		//Sanitise SQL based on single field name as object
		public object Sanitise(object field)
		{
			switch (Type.GetTypeCode(field.GetType()))
			{
				//only string fields can be used for injection
				case TypeCode.String:
					Sanitise(field.ToString());
					return field;
				//return field as is otherwise
				default:
					return field;
			}
		}

		//Sanitise SQL based on single field name as string
		public string Sanitise(string field)
		{
			if (field == null) return "";
			var injectedChars = _SanitiseNonWords.Any(s => field.Contains(s));
			var punctuation = field.Where(Char.IsPunctuation).Distinct().ToArray();
			var words = field.Split().Select(x => x.Trim(punctuation));
			var injectedWords = _SanitiseWords.Any(s => words.Contains(s, StringComparer.OrdinalIgnoreCase));

			if (injectedChars || injectedWords)
				throw new Exception($"SQL contains injected text ({field})");
			else
				return field;
		}

		private readonly List<string> _SanitiseWords = new List<string>
		{
			"select",
			"drop",
			"from",
			"insert",
			"exec",
			"execute",
			"alter",
			"create",
			"declare",
			"cursor",
			"begin",
			"cast",
			"delete",
			"end",
			"fetch",
			"kill",
			"sys",
			"syscolumns",
			"sysobjects"
		};

		private readonly List<string> _SanitiseNonWords = new List<string>
		{
			";",
			"–",
			",",
			".",
			"/",
			"*",
			"*/",
			"/*",
			"@",
			"@@",
		};

		public async Task<bool> BulkInsert<T>(IEnumerable<T> model, OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated
		{
			try
			{
				DataTable dt = null;
				foreach (var d in model)
				{
					if (dt == null)
						dt = d.GenerateDataTable(owner);
					d.InsertRow(dt);
				}

				using (var rep = new SqlRepository(_Configuration, _Logger))
					await rep.BulkInsert<T>(dt);
			}
			catch (Exception e)
			{
				_Logger.LogError(e, $"{e.Message}");
				return false;
			}
			return true;
		}

		public Task<IEnumerable<ReactInputOption>> GetConstrainedTableValuesAsync(string tableName, string valueColumn, string labelColumn)
		{
			return Task.Run(() =>
			{
				var sql = new StringBuilder();

				sql.AppendLine($"SELECT {valueColumn} as ValueColumn ,{labelColumn} as LabelColumn");
				sql.AppendLine($"FROM [dbo].[{tableName}]");

				var data = ExecuteSql<DictionaryTable>(sql.ToString(), new List<SqlParameter> { })?.OrderBy(d => d.LabelColumn);

				var ret = data.Select(d => new ReactInputOption { Value = d.ValueColumn, Label = d.LabelColumn });
				return ret ?? new List<ReactInputOption>();
			});
		}

		public void ExecuteProcedure(string procedureName, Dictionary<string, object> parameters)
		{
			try
			{
				var sql = new StringBuilder();
				sql.AppendLine($"EXEC {procedureName}");

				var sqlParams = new List<SqlParameter>();

				if (parameters != null)
				{
					bool first = true;
					foreach (var param in parameters)
					{
						sql.AppendLine($"{(first ? "" : ",")} @{param.Key}");
						first = false;
						sqlParams.Add(new SqlParameter($"@{param.Key}", param.Value));
					}
				}

				using (var rep = new SqlRepository(_Configuration, _Logger))
					rep.ExecuteProcedure(sql.ToString(), sqlParams);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public IEnumerable<T> ExecuteProcedure<T>(string procedureName, Dictionary<string, object> parameters) where T : IOrmGenerated
		{
			var sql = new StringBuilder();
			sql.AppendLine($"EXEC {procedureName}");

			var sqlParams = new List<SqlParameter>();

			if (parameters != null)
			{
				bool first = true;
				foreach (var param in parameters)
				{
					sql.AppendLine($"{(first ? "" : ",")} @{param.Key}");
					first = false;
					sqlParams.Add(new SqlParameter($"@{param.Key}", param.Value));
				}
			}

			using (var rep = new SqlRepository(_Configuration, _Logger))
				return rep.ExecuteSql<T>(sql.ToString(), sqlParams) ?? new List<T>();
		}

		public async Task BulkUpdateAsync<T>(IEnumerable<T> model, string primaryColumnName = "", OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated
		{
			DataTable dt = null;
			foreach (var d in model)
			{
				if (dt == null)
					dt = d.GenerateDataTable(owner);
				d.InsertRow(dt);
			}

			if (!string.IsNullOrEmpty(primaryColumnName))
			{
				var pk = new DataColumn[] { dt.Columns[primaryColumnName] };
				dt.PrimaryKey = pk;
			}

			await BulkUpdateAsync(dt, primaryColumnName);
		}

		public int BulkUpdateSync<T>(IEnumerable<T> model, string primaryColumnName = "", OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated
		{
			DataTable dt = null;
			foreach (var d in model)
			{
				if (dt == null)
					dt = d.GenerateDataTable(owner);
				d.InsertRow(dt);
			}

			if (!string.IsNullOrEmpty(primaryColumnName))
			{
				var pk = new DataColumn[] { dt.Columns[primaryColumnName] };
				dt.PrimaryKey = pk;
			}

			return BulkUpdateSync(dt, primaryColumnName);
		}

		public async Task BulkUpdateAsync(DataTable dataTable, string primaryColumnName = "")
		{
			var tempTableName = "#temp" + dataTable.TableName.Replace(".", "");

			var tempTableSql = new StringBuilder();
			tempTableSql.AppendLine("Create table " + tempTableName + " (");
			bool first = true;
			foreach (DataColumn col in dataTable.Columns)
			{
				if (!first)
					tempTableSql.Append(",");

				first = false;
				tempTableSql.AppendLine("[" + col.ColumnName + "] " + GetSqlType(col.DataType, col.MaxLength));
			}

			tempTableSql.AppendLine(")");

			var updateSql = new StringBuilder();
			updateSql.Append("UPDATE t SET ");
			first = true;
			foreach (DataColumn col in dataTable.Columns)
			{
				if (dataTable.PrimaryKey.Contains(col) || (!string.IsNullOrEmpty(primaryColumnName)) && col.ColumnName == primaryColumnName)
					continue;

				if (!first)
					updateSql.Append(",");
				first = false;
				updateSql.AppendLine("t.[" + col.ColumnName + "] = tmp.[" + col.ColumnName + "] ");
			}

			updateSql.AppendLine("FROM " + dataTable.TableName + " t INNER JOIN " + tempTableName + " tmp ON ");
			first = true;
			if (string.IsNullOrEmpty(primaryColumnName))
			{
				foreach (DataColumn col in dataTable.PrimaryKey)
				{
					if (!first)
						updateSql.Append(" AND ");
					first = false;
					updateSql.Append("tmp.[" + col.ColumnName + "] = t.[" + col.ColumnName + "] ");
				}
			}
			else
			{
				updateSql.Append("tmp.[" + primaryColumnName + "] = t.[" + primaryColumnName + "] ");
			}

			//Use sqlBulkCopy to update database
			using (SqlConnection conn = new SqlConnection(_Configuration.GetConnectionString("DefaultConnection")))
			{
				await conn.OpenAsync();//.Open();

				try
				{
					//Create temp table
					using (var cmd = new SqlCommand(tempTableSql.ToString(), conn))
					{
						await cmd.ExecuteNonQueryAsync();

						using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
						{
							bulkCopy.DestinationTableName = tempTableName;
							bulkCopy.ColumnMappings.Clear();

							foreach (DataColumn col in dataTable.Columns)
							{
								bulkCopy.ColumnMappings.Add("[" + col.ColumnName + "]", "[" + col.ColumnName + "]");
							}

							await bulkCopy.WriteToServerAsync(dataTable);
							bulkCopy.Close();
						}

						// Merge from temp table into main table
						// cmd = new SqlCommand(updateSql.ToString(), conn);
						cmd.CommandText = updateSql.ToString();
						await cmd.ExecuteNonQueryAsync();
					}
				}
				catch (SqlException ex)
				{
					throw ex;
				}
				finally
				{
					conn.Close();
				}
			}
		}

		public int BulkUpdateSync(DataTable dataTable, string primaryColumnName = "")
		{
			var tempTableName = "#temp" + dataTable.TableName.Replace(".", "");

			var tempTableSql = new StringBuilder();
			tempTableSql.AppendLine("Create table " + tempTableName + " (");
			bool first = true;
			foreach (DataColumn col in dataTable.Columns)
			{
				if (!first)
					tempTableSql.Append(",");

				first = false;
				tempTableSql.AppendLine("[" + col.ColumnName + "] " + GetSqlType(col.DataType, col.MaxLength));
			}

			tempTableSql.AppendLine(")");

			var updateSql = new StringBuilder();
			updateSql.Append("UPDATE t SET ");
			first = true;
			foreach (DataColumn col in dataTable.Columns)
			{
				if (dataTable.PrimaryKey.Contains(col) || (!string.IsNullOrEmpty(primaryColumnName)) && col.ColumnName == primaryColumnName)
					continue;

				if (!first)
					updateSql.Append(",");

				first = false;
				updateSql.AppendLine("t.[" + col.ColumnName + "] = tmp.[" + col.ColumnName + "] ");
			}

			updateSql.AppendLine("FROM " + dataTable.TableName + " t INNER JOIN " + tempTableName + " tmp ON ");
			first = true;
			if (string.IsNullOrEmpty(primaryColumnName))
			{
				foreach (DataColumn col in dataTable.PrimaryKey)
				{
					if (!first)
						updateSql.Append(" AND ");
					first = false;
					updateSql.Append("tmp.[" + col.ColumnName + "] = t.[" + col.ColumnName + "] ");
				}
			}
			else
				updateSql.Append("tmp.[" + primaryColumnName + "] = t.[" + primaryColumnName + "] ");

			//Use sqlBulkCopy to update database
			using SqlConnection conn = new SqlConnection(_Configuration.GetConnectionString("DefaultConnection"));
			conn.Open();

			try
			{
				//Create temp table
				using var cmd = new SqlCommand(tempTableSql.ToString(), conn);
				cmd.ExecuteNonQuery();

				using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
				{
					bulkCopy.DestinationTableName = tempTableName;
					bulkCopy.ColumnMappings.Clear();

					foreach (DataColumn col in dataTable.Columns)
						bulkCopy.ColumnMappings.Add("[" + col.ColumnName + "]", "[" + col.ColumnName + "]");

					bulkCopy.WriteToServer(dataTable);
					bulkCopy.Close();
				}

				// Merge from temp table into main table
				// cmd = new SqlCommand(updateSql.ToString(), conn);
				cmd.CommandText = updateSql.ToString();
				return cmd.ExecuteNonQuery();
			}
			catch (SqlException)
			{
				throw;
			}
			finally
			{
				conn.Close();
			}
		}

		static string GetSqlType(Type t, int maxLength)
		{
			switch (t.Name)
			{
				case "String": return $"nvarchar({(maxLength == -1 ? "max" : maxLength.ToString())})";
				case "Int32": return "int";
				case "Single": return "decimal(18,12)";
				case "Double": return "float";
				case "Decimal": return "decimal(18,12)";
				case "Boolean": return "Bit";
				case "DateTime": return "DateTime";
				case "Int16": return "smallint";
				case "Int64": return "bigint";
				case "Byte[]": return "varbinary(max)";
			}

			return "";
		}

		public async Task InsertFromDataTable<T>(DataTable dt) where T : IOrmGenerated
		{
			try
			{
				using (var agg = new SqlRepository(_Configuration, _Logger))
					await agg.BulkInsert<T>(dt);
			}
			catch
			{
			}
		}
	}

	public static class GeneralDataRepositoryExtensions
	{
		public static string GetDataConditional(this DataConditional cond)
		{
			var conditionals = new string[] { "=", ">", "<", ">=", "<=", "<>", "IS NOT NULL", "IS NULL", "IN" };
			return conditionals[(int)cond];
		}
	}

	public class GeneralDataParameter
	{
		/// <summary>This method is for grouping all conditions that follow, if openGroup is set to false then it closes the group, if true it opens a new grouping
		/// <example>example:
		/// <code>
		/// var plan = _GeneralDataRepository.GetWhere<Plan>(new List<GeneralDataParameter> {
		///         new GeneralDataParameter("Date",date),
		///         new GeneralDataParameter(true, DataParameterType.Or) <- this is the start of a group and will have an 'OR' before it opens
		///            new GeneralDataParameter("Id",1)
		///            new GeneralDataParameter("Id",2)
		///            new GeneralDataParameter("Id",3)
		///         new GeneralDataParameter(false) <- this is the end of the group
		///     })?.FirstOrDefault();
		/// </code>
		/// gets records that match a date or Id 1, 2 or 3:
		/// </example>
		/// </summary>
		public GeneralDataParameter(string field, object value, DataConditional conditional = DataConditional.EqualTo, DataParameterType multiConditionType = DataParameterType.And)
		{
			Field = field;
			Value = value;
			Conditional = conditional;
			MultiConditionType = multiConditionType;
		}

		/// <summary>This method is for grouping all conditions that follow, if openGroup is set to false then it closes the group, if true it opens a new grouping
		/// <example>example:
		/// <code>
		/// var plan = _GeneralDataRepository.GetWhere<Plan>(new List<GeneralDataParameter> {
		///         new GeneralDataParameter("Date",date),
		///         new GeneralDataParameter(true, DataParameterType.Or) <- this is the start of a group and will have an 'OR' before it opens
		///            new GeneralDataParameter("Id",1)
		///            new GeneralDataParameter("Id",2)
		///            new GeneralDataParameter("Id",3)
		///         new GeneralDataParameter(false) <- this is the end of the group
		///     })?.FirstOrDefault();
		/// </code>
		/// gets records that match a date or Id 1, 2 or 3:
		/// </example>
		/// </summary>
		public GeneralDataParameter(bool openGroup, DataParameterType multiConditionType = DataParameterType.And)
		{
			MultiConditionType = multiConditionType;
			IsGroup = true;
			OpenGroup = openGroup;
		}

		public string Field { get; set; }
		public object Value { get; set; }
		public DataConditional Conditional { get; set; } = DataConditional.EqualTo;
		public DataParameterType MultiConditionType { get; set; } = DataParameterType.And;
		public bool IsGroup { get; set; } = false;
		public bool OpenGroup { get; set; } = false;
	}

	public enum DataConditional
	{
		EqualTo,
		GreaterThan,
		LessThan,
		GreaterThanOrEqualTo,
		LessThanOrEqualTo,
		NotEqualTo,
		NotNull,
		IsNull,
		In
	}

	public enum DataParameterType
	{
		And,
		Or
	}
}
