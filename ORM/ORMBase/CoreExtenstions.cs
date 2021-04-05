using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMBase
{
    public static class CoreExtenstions
    {
        public static List<T> MapDataToBusinessEntityCollection<T>(this IDataReader reader) where T : IOrmGenerated
        {
            Type businessEntityType = typeof(T);
            List<T> entitys = new List<T>();
            Hashtable hastable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();

            foreach (PropertyInfo info in properties)
            {
                hastable[info.Name.ToUpper()] = info;
            }

            while (reader.Read())
            {
                T newObject = Activator.CreateInstance<T>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo info = (PropertyInfo)hastable[reader.GetName(i).ToUpper()];
                    if (info != null && info.CanWrite)
                    {
                        if (reader.IsDBNull(i))
                            continue;

                        if (reader.GetDataTypeName(i).EndsWith("hierarchyid"))
                            info.SetValue(newObject, reader.GetValue(i).ToString(), null);
                        else
                            info.SetValue(newObject, reader.GetValue(i), null);
                    }
                }

                entitys.Add(newObject);
            }

            reader.Close();
            return entitys;
        }

        public static T MapDataToBusinessEntity<T>(this IDataReader reader) where T : new()
        {
            Type businessEntityType = typeof(T);
            T entity = new T();
            Hashtable hastable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();

            foreach (PropertyInfo info in properties)
            {
                hastable[info.Name.ToUpper()] = info;
            }
            bool set = false;
            while (reader.Read())
            {
                //T newObject = new T();
                set = true;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo info = (PropertyInfo)hastable[reader.GetName(i).ToUpper()];
                    if (info != null && info.CanWrite)
                    {
                        if (reader.IsDBNull(i))
                            continue;
                        info.SetValue(entity, reader.GetValue(i), null);
                    }
                }

                // entitys.Add(newObject);
            }

            reader.Close();

            return set ? entity : default(T);
        }

        public static List<T> MapDatatable<T>(this DataTable dt)
        {
            var data = new List<T>();

            foreach(DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T> (DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName && dr[column.ColumnName] != DBNull.Value)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

    }
}
