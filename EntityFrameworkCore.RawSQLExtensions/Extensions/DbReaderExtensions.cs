using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class DbReaderExtensions
    {
        public static IDictionary<string, DbColumn> GetSchema<T>(this DbDataReader dr)
        {
            IDictionary<string, DbColumn> valuePairs;
            if (typeof(T).IsTupleType())
            {
                var props = typeof(T).GetRuntimeFields();
                valuePairs = dr.GetColumnSchema()          
               .ToDictionary(key => key.ColumnName.ToLower());
            }
           else
            {
                var props = typeof(T).GetRuntimeProperties();
                valuePairs = dr.GetColumnSchema()
               .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
               .ToDictionary(key => key.ColumnName.ToLower());
            }
            return valuePairs;
        }
      
        public static T MapObject<T>(this DbDataReader dr, IDictionary<string, DbColumn> colMapping)
        {
            if (typeof(T).IsSqlSimpleType())
            {
                return (T)dr.GetValue(0);
            }
            else
            {
                T obj = Activator.CreateInstance<T>();
                if (typeof(T).IsTupleType())
                {
                    var fields = typeof(T).GetRuntimeFields().ToArray();
                    //https://stackoverflow.com/questions/59000557/valuetuple-set-fields-via-reflection
                    object xobj = obj;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var val = Convert.ChangeType(dr.GetValue(i), fields[i].FieldType);
                       fields[i].SetValue(xobj, val == DBNull.Value ? null : val);
                    }
                    obj = (T)Convert.ChangeType( xobj,typeof(T));
                }
                else
                {
                    IEnumerable<PropertyInfo> props = typeof(T).GetRuntimeProperties();
                    foreach (var prop in props)
                    {
                        var propName = prop.Name.ToLower();
                        if (colMapping.ContainsKey(propName))
                        {
                            var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                            prop.SetValue(obj, val == DBNull.Value ? null : val);
                        }
                        else
                        {
                            prop.SetValue(obj, null);
                        }
                    }
                }

                return obj;
            }
        }
     

        public static async Task<IList<T>> ToListAsync<T>(this DbDataReader dr)
        {
            var objList = new List<T>();

            var colMapping = dr.GetSchema<T>();

            if (dr.HasRows)
                while (await dr.ReadAsync())
                    objList.Add(dr.MapObject<T>(colMapping));

            return objList;
        }

        public static IList<T> ToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
                while (dr.Read())
                    objList.Add(dr.MapObject<T>(colMapping));

            return objList;
        }

        public static DataTable ToDataTable(this DbDataReader dr)
        {
            DataTable objDataTable = new DataTable();
            for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
            {
                objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
            }
            if (dr.HasRows)
            {
                objDataTable.BeginLoadData();
                object[] objValues = new object[dr.FieldCount];
                while (dr.Read())
                {
                    dr.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                objDataTable.EndLoadData();
            }
            return objDataTable;
        }

        public static async Task<DataTable> ToDataTableAsync(this DbDataReader dr)
        {
            DataTable objDataTable = new DataTable();
            for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
            {
                objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
            }
            if (dr.HasRows)
            {
                objDataTable.BeginLoadData();
                object[] objValues = new object[dr.FieldCount];
                while (await dr.ReadAsync())
                {
                    dr.GetValues(objValues);
                    objDataTable.LoadDataRow(objValues, true);
                }
                objDataTable.EndLoadData();
            }
            return objDataTable;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
                while (await dr.ReadAsync())
                    return dr.MapObject<T>(colMapping);

            return default(T);
        }

        public static T FirstOrDefault<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            if (dr.HasRows)
                while (dr.Read())
                    return dr.MapObject<T>(colMapping);

            return default(T);
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();

            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (await dr.ReadAsync())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping);
                    hasResult = true;
                }

            return obj;
        }
        
        public static T SingleOrDefault<T>(this DbDataReader dr)
        {
            var colMapping = dr.GetSchema<T>();
            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (dr.Read())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping);
                    hasResult = true;
                }

            return obj;
        }
    }
}