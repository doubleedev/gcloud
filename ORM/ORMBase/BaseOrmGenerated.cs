using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMBase
{
    public abstract class BaseOrmGenerated : IOrmGenerated
    {
        protected string TableName;
        protected string TableOwner;
        string _ConnectionStringName = "DefaultConnection";

        public BaseOrmGenerated()
        {
            if (this.GetType().GetMethod("ReConstruct") != null)
                this.GetType().InvokeMember("ReConstruct", System.Reflection.BindingFlags.InvokeMethod, null, this, null);

        }

        public string GetTableName()
        {
            return TableName;
        }

        public string TableReference
        {
            get
            {
                return string.Format("{0}.[{1}]", TableOwner, TableName);
            }
        }
        public abstract List<SqlParameter> BaseWhereParameters();

        public abstract List<SqlParameter> InsertParameters();

        public abstract void Map(IDataReader dataReader);

        public abstract string SqlSelect(bool withPkWhere);

        public abstract string SqlSelectPage(int pageSize, int page, bool withPkWhere);

        public abstract List<SqlParameter> UpdateParameters();

        public virtual string GetConnectionString() => _ConnectionStringName;

        public abstract string SqlInsert();
        public abstract string SqlUpdate();
        public abstract string SqlDelete();

        public static DateTime MinTime
        {
            get
            {
                return new DateTime(1900, 0, 0, 0, 0, 0);
            }
        }

        public static DateTime MinDate
        {
            get
            {
                return new DateTime(1753, 1, 1);
            }
        }

        public static int DbToInt(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return Convert.ToInt32(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static int? DbToIntNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToInt32(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static short? DbToShortNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToInt16(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static short DbToInt16(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return Convert.ToInt16(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static short? DbToInt16Nullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToInt16(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static byte[] DbToByteArray(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return value as byte[];
            }
            catch
            {
                return null;
            }
        }
        
        public static bool DbToBool(object value)
        {
            if (Convert.IsDBNull(value))
                return false;

            try
            {
                return Convert.ToBoolean(value.ToString());
            }
            catch { }

            try
            {
                return Math.Abs(Convert.ToInt32(value.ToString())) == 1;
            }
            catch { }

            try
            {
                return value.ToString().ToLower().StartsWith("y");
            }
            catch { }

            return false;
        }

        public static bool? DbToBoolNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;

            try
            {
                return Convert.ToBoolean(value.ToString());
            }
            catch { }

            try
            {
                return Math.Abs(Convert.ToInt32(value.ToString())) == 1;
            }
            catch { }

            try
            {
                return value.ToString().ToLower().StartsWith("y");
            }
            catch { }

            return null;
        }

        public static float DbToFloat(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return (float)Convert.ToDouble(value.ToString());
            }
            catch
            {
                return (float)0;
            }
        }

        public static float? DbToFloatNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return (float)Convert.ToDouble(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static double DbToDouble(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return Convert.ToDouble(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static double? DbToDoubleNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToDouble(value.ToString());
            }
            catch
            {
                return null;
            }
        }
        public static long DbToLong(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return Convert.ToInt64(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static long? DbToLongNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToInt64(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static decimal DbToDecimal(object value)
        {
            if (Convert.IsDBNull(value))
                return 0;
            try
            {
                return Convert.ToDecimal(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static decimal? DbToDecimalNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return Convert.ToDecimal(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public static string DbToStringRTrim(object value)
        {
            return DbToString(value).TrimEnd(" ".ToCharArray());
        }

        public static string DbToString(object value)
        {
            if (Convert.IsDBNull(value))
                return null;
            try
            {
                return value.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string DbHierarchyIdToString(object value)
        {
            if (Convert.IsDBNull(value))
                return string.Empty;
            try
            {
                return value.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static DateTime DbToDateTime(object value)
        {
            if (Convert.IsDBNull(value))
                return System.DateTime.MinValue;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }

        public static DateTime? DbToDateTimeNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime DbToTime(object value)
        {
            if (Convert.IsDBNull(value))
                return System.DateTime.MinValue;

            try
            {
                // Assume that the value is a timespan
                return new DateTime(MinDate.Year, MinDate.Month, MinDate.Day).Add((TimeSpan)value);
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }

        public static DateTime? DbToTimeNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;

            try
            {
                // Assume that the value is a timespan
                return new DateTime(MinDate.Year, MinDate.Month, MinDate.Day).Add((TimeSpan)value);
            }
            catch
            {
                return null;
            }
        }

        public static TimeSpan DbToTimeSpan(object value)
        {
            if (Convert.IsDBNull(value))
                return System.TimeSpan.MinValue;

            try
            {
                return (TimeSpan)value;
            }
            catch
            {
                return System.TimeSpan.MinValue;
            }
        }

        public static TimeSpan? DbToTimeSpanNullable(object value)
        {
            if (Convert.IsDBNull(value))
                return null;

            try
            {
                return (TimeSpan)value;
            }
            catch
            {
                return null;
            }
        }


        public void InsertRow(DataTable dt)
        {
            var row = dt.NewRow();
            var fields = InsertParameters();
            fields.AddRange(UpdateParameters());
            var allfields = fields.GroupBy(f => f.ParameterName);
            foreach (var fieldGrp in allfields)
            {
                var field = fieldGrp.First();
                var fieldName = field.ParameterName.Substring(1);
                var fieldValue = this.GetType().GetProperty(fieldName).GetValue(this, null);

                if (field.DbType == DbType.DateTime && field.IsNullable && (fieldValue == null || (DateTime)fieldValue == default(DateTime)))
                    row[fieldName] = DBNull.Value;
                else if (field.IsNullable && fieldValue == null)
                    row[fieldName] = DBNull.Value;
                else
                    row[fieldName] = fieldValue;
            }
            dt.Rows.Add(row);
        }

        public DataTable GenerateDataTable(OrmTableOwner owner = OrmTableOwner.dbo)
        {
            var dt = new DataTable($"{owner.ToString()}.{GetTableName()}");
            var fields = InsertParameters();
            fields.AddRange(UpdateParameters());
            var allfields = fields.GroupBy(f => f.ParameterName);
            foreach (var fieldGrp in allfields)
            {
                var field = fieldGrp.First();
                var fieldName = field.ParameterName.Substring(1);
                var propertyType = GetFieldType(field.DbType);
                var isNullable = field.IsNullable;
                if (field.SqlDbType == SqlDbType.NVarChar && field.Size > 0)
                    dt.Columns.Add(new DataColumn(fieldName, propertyType) { MaxLength = field.Size, AllowDBNull = isNullable });
                else
                    dt.Columns.Add(new DataColumn(fieldName, propertyType) { AllowDBNull = isNullable });
            }
            return dt;
        }


        private Type GetFieldType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.String:
                    return typeof(string);
                case DbType.Int32:
                case DbType.Int64:
                    return typeof(int);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Binary:
                    return typeof(byte[]);
                case DbType.DateTime:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Double:
                    return typeof(double);
                default:
                    throw new TypeInitializationException(dbType.ToString(), new Exception("Unhandled DbType"));
            }
        }

        #region IDisposable Support
        private bool _DisposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
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

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseGenerated() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public enum OrmTableOwner
    {
        dbo,
        staging
    }
}
