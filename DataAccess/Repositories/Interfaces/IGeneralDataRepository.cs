using ORMBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public interface IGeneralDataRepository
	{
		T GetSingle<T>(string field, object value, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated;
		IEnumerable<T> GetAll<T>() where T : IOrmGenerated;
		IEnumerable<T> GetWhere<T>(string field, object value, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated;
		IEnumerable<T> GetWhere<T>(Dictionary<string, object> parameters, DataConditional conditional = DataConditional.EqualTo) where T : IOrmGenerated;
		IEnumerable<T> GetWhere<T>(IEnumerable<GeneralDataParameter> parameters, IEnumerable<string> inParameters = null) where T : IOrmGenerated;
		void Update<T>(T model) where T : IOrmGenerated;
		void Delete<T>(T model) where T : IOrmGenerated;
		void BulkDelete<T>(IEnumerable<T> model, IEnumerable<string> fields, OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated;
		object Insert<T>(T model) where T : IOrmGenerated;
		long Count<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;
		IEnumerable<T> ExecuteSql<T>(string sql, List<SqlParameter> parameters) where T : IOrmGenerated;
		IEnumerable<T> ExecuteProcedure<T>(string procedureName, Dictionary<string, object> parameters) where T : IOrmGenerated;
		Task<bool> BulkInsert<T>(DataTable dt) where T : IOrmGenerated;
		Task<bool> BulkInsert<T>(IEnumerable<T> model, OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated;
		Task BulkUpdateAsync(DataTable dt, string primaryColumnName = "");
		Task BulkUpdateAsync<T>(IEnumerable<T> model, string primaryColumnName = "", OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated;

		void Truncate<T>(OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated;

		int BulkUpdateSync<T>(IEnumerable<T> model, string primaryColumnName = "", OrmTableOwner owner = OrmTableOwner.dbo) where T : IOrmGenerated;
		int BulkUpdateSync(DataTable dataTable, string primaryColumnName = "");
		void ExecuteProcedure(string procedureName, Dictionary<string, object> parameters);

		Task InsertFromDataTable<T>(DataTable dt) where T : IOrmGenerated;
		Task<IEnumerable<ReactInputOption>> GetConstrainedTableValuesAsync(string tableName, string valueColumn, string labelColumn);
		Task<IEnumerable<D>> GetDetailForHeaders<H, D>(IEnumerable<H> headers, string headerIdColumn, string detailIdColumn) where H : IOrmGenerated where D : IOrmGenerated;


		//Sanitise SQL based on data table
		public DataTable Sanitise(DataTable dt);
		//Sanitise SQL based on generalDataParameters
		public IEnumerable<GeneralDataParameter> Sanitise(IEnumerable<GeneralDataParameter> parameters);
		//Sanitise SQL based on SqlParameters
		public IEnumerable<SqlParameter> Sanitise(IEnumerable<SqlParameter> parameters);
		//Sanitise SQL based on dictionay fields
		public Dictionary<string, object> Sanitise(Dictionary<string, object> fields);
		//Sanitise SQL based field names
		public IEnumerable<string> Sanitise(IEnumerable<string> fields);
		//Sanitise SQL based on single field name as object
		public object Sanitise(object field);
		//Sanitise SQL based on single field name as string
		public string Sanitise(string field);
	}
}
