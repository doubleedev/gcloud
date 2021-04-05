/* AUTO-GENERATED CODE 
* This file was generated using the SEFORM Code Generation tool 
BY: ktras
ON: 03/04/2021 20:01
Using Build 1.0.17.1
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ORMBase;

namespace DataAccess
{
	public partial class Users : BaseOrmGenerated
	{

		#region Private Fields
		string _Name = null; //Maps to Database Column [Name]

		#endregion Private Fields
		public Users()
		{
			TableOwner = "dbo";
			TableName = "Users";
		}


		#region Public Properties
		public int Id { get; set; } = 0;

		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				if (value != null && value.Length > 50)
					_Name = value.Substring(0, 50);
				else
					_Name = value;
			}
		}


		#endregion Public Properties

		public override List<SqlParameter> BaseWhereParameters()
		{
			return new List<SqlParameter>
			{
				new SqlParameter("@Id",Id)
			};
		}

		public override List<SqlParameter> InsertParameters()
		{
			return new List<SqlParameter>
			{
				new SqlParameter("@Name",Name)
			};
		}

		public override List<SqlParameter> UpdateParameters()
		{
			return new List<SqlParameter>
			{
				new SqlParameter("@Id",Id ),
				new SqlParameter("@Name",Name )
			};
		}

		public override string SqlSelect(bool withPkWhere)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"SELECT ");
			sql.AppendLine("runSql.[Id], ");
			sql.AppendLine("runSql.[Name]");
			sql.AppendLine($"FROM {TableReference} as runSql ");

			if (withPkWhere)
			{
				sql.AppendLine("WHERE [Id] = @Id ");
			}
			return sql.ToString();
		}

		public override string SqlSelectPage(int pageSize, int page, bool sortFieldName)
		{
			throw new NotImplementedException();
		}

		public override void Map(IDataReader dataReader)
		{
			Id = DbToInt(dataReader["Id"]);
			Name = DbToString(dataReader["Name"]);
		}

		public override string SqlInsert()
		{
			var sql = new StringBuilder();
			sql.AppendLine($"INSERT INTO {TableReference}(");
			sql.AppendLine("[Name]");
			sql.AppendLine(") VALUES (");
			sql.AppendLine("@Name");
			sql.AppendLine(")");
			return sql.ToString();
		}

		public override string SqlUpdate()
		{
			var sql = new StringBuilder();
			sql.AppendLine($"UPDATE {TableReference}");
			sql.AppendLine($"SET ");
			sql.AppendLine("[Name] = @Name");
			sql.AppendLine("WHERE [Id] = @Id ");
			return sql.ToString();
		}

		public override string SqlDelete()
		{
			var sql = new StringBuilder();
			sql.AppendLine($"DELETE FROM {TableReference} WHERE");
			sql.AppendLine("[Id] = @Id ");
			return sql.ToString();
		}
	}
}