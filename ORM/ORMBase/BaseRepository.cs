using Microsoft.Extensions.Logging;
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
	public abstract class BaseRepository : IRepository
	{
		private ILogger<IRepository> _Logger;
		public BaseRepository(ILogger<IRepository> logger)
		{
			_Logger = logger;
		}
		public abstract long Count<T>() where T : IOrmGenerated;

		public abstract long Count<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;

		public abstract void Delete<T>(T model) where T : IOrmGenerated;
		public abstract void Truncate<T>(string schema, string TableName) where T : IOrmGenerated;

		public abstract List<T> ExecuteSql<T>(string sqlStatements, List<SqlParameter> parameters) where T : IOrmGenerated;

		public abstract void ExecuteProcedure(string sqlStatements, List<SqlParameter> parameters, string connectionName = "DefaultConnection");

		public abstract T ExecuteSqlSingle<T>(string sqlStatements, List<SqlParameter> parameters) where T : IOrmGenerated;
		public abstract List<T> GetAll<T>() where T : IOrmGenerated;
		public abstract List<T> GetAll<T>(int pageSize, int page) where T : IOrmGenerated;
		public abstract T GetSingle<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;

		public abstract List<T> GetWhere<T>(Dictionary<string, object> parameters) where T : IOrmGenerated;
		public abstract List<T> GetWhere<T>(Expression<Func<T, bool>> where) where T : IOrmGenerated;

		public abstract Task BulkInsert<T>(DataTable dt) where T : IOrmGenerated;

		public abstract object Insert<T>(T model) where T : IOrmGenerated;

		public abstract void Update<T>(T model) where T : IOrmGenerated;

		private bool _DisposedValue = false; // To detect redundant calls

		protected void Dispose(bool disposing)
		{
			if (!_DisposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_DisposedValue = true;
			}
		}

		protected void Log(string message, LogLevel logLevel, string file, Exception e = null)
		{
			if (_Logger != null)
			{
				if (e == null)
					_Logger.Log(logLevel, message, file);
				else
					_Logger.LogError(e, message, file);

			}
		}
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
	}
}
