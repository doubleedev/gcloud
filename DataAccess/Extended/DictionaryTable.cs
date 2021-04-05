using ORMBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess
{
	public partial class DictionaryTable : BaseOrmGenerated
	{

		#region Private Fields
		object _ValueColumn = null; //Maps to Database Column [ValueColumn]
		object _LabelColumn = null; //Maps to Database Column [LabelColumn]

		#endregion Private Fields
		public DictionaryTable()
		{
			TableOwner = "dbo";
			TableName = "DictionaryTable";
		}


		#region Public Properties
		public object ValueColumn { get; set; }

		public object LabelColumn { get; set; }

		#endregion Public Properties

		public override List<SqlParameter> BaseWhereParameters()
		{
			return new List<SqlParameter>();
		}

		public override List<SqlParameter> InsertParameters()
		{
			return new List<SqlParameter>();
		}

		public override List<SqlParameter> UpdateParameters()
		{
			return new List<SqlParameter>();
		}

		public override string SqlSelect(bool withPkWhere)
		{
			return string.Empty;
		}

		public override string SqlSelectPage(int pageSize, int page, bool withPkWhere)
		{
			throw new NotImplementedException();
		}

		public override void Map(IDataReader dataReader)
		{
		}

		public override string SqlInsert()
		{
			return string.Empty;
		}

		public override string SqlUpdate()
		{
			return string.Empty;
		}

		public override string SqlDelete()
		{
			return string.Empty;
		}
	}
}