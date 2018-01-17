using System;
using System.Collections.Generic;
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
            var props = typeof(T).GetRuntimeProperties();

            return dr.GetColumnSchema()
                    .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                    .ToDictionary(key => key.ColumnName.ToLower());
        }

        public static T MapObject<T>(this DbDataReader dr, IDictionary<string, DbColumn> colMapping, IEnumerable<PropertyInfo> props)
        {
            if (typeof(T).IsSqlSimpleType())
            {
                return (T)dr.GetValue(0);
            }
            else
            {
                T obj = Activator.CreateInstance<T>();

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

                return obj;
            }
        }

        public static async Task<IList<T>> ToListAsync<T>(this DbDataReader dr)
        {
            var objList = new List<T>();

            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            if (dr.HasRows)
                while (await dr.ReadAsync())
                    objList.Add(dr.MapObject<T>(colMapping, props));

            return objList;
        }
        public static IList<T> ToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();

            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            if (dr.HasRows)
                while (dr.Read())
                    objList.Add(dr.MapObject<T>(colMapping, props));

            return objList;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this DbDataReader dr)
        {
            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            if (dr.HasRows)
                while (await dr.ReadAsync())
                    return dr.MapObject<T>(colMapping, props);

            return default(T);
        }

        public static T FirstOrDefault<T>(this DbDataReader dr)
        {
            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            if (dr.HasRows)
                while (dr.Read())
                    return dr.MapObject<T>(colMapping, props);

            return default(T);
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this DbDataReader dr)
        {
            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (await dr.ReadAsync())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping, props);
                    hasResult = true;
                }

            return obj;
        }


        public static T SingleOrDefault<T>(this DbDataReader dr)
        {
            var props = typeof(T).GetRuntimeProperties();
            var colMapping = dr.GetSchema<T>();

            T obj = default(T);
            bool hasResult = false;

            if (dr.HasRows)
                while (dr.Read())
                {
                    if (hasResult)
                        throw new InvalidOperationException("Sequence contains more than one matching element");

                    obj = dr.MapObject<T>(colMapping, props);
                    hasResult = true;
                }

            return obj;
        }
    }
}